using Adorika.Domain.Entities.Base;
using Adorika.Domain.Enums;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a multi-factor authentication method configured for a user.
/// Supports multiple MFA types: Authenticator (TOTP), SMS, and Email.
/// </summary>
public class MfaMethod : FullAuditedEntity
{

    // ===== USER ASSOCIATION =====
    public Guid UserId { get; set; }

    // ===== MFA METHOD TYPE =====
    /// <summary>
    /// The type of MFA method.
    /// </summary>
    public MfaMethodType MethodType { get; set; }

    // ===== STATUS =====
    /// <summary>
    /// Indicates if this MFA method is enabled and active.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Indicates if this is the primary/preferred MFA method for the user.
    /// Only one method can be primary per user.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Indicates if the method has been verified by the user.
    /// </summary>
    public bool IsVerified { get; set; }

    // ===== AUTHENTICATOR (TOTP) DATA =====
    /// <summary>
    /// The secret key for TOTP generation (encrypted in storage).
    /// Used for authenticator apps like Google Authenticator, Microsoft Authenticator, etc.
    /// </summary>
    public string? AuthenticatorKey { get; set; }

    /// <summary>
    /// The secret key (alias for consistency).
    /// </summary>
    public string? SecretKey
    {
        get => AuthenticatorKey;
        set => AuthenticatorKey = value;
    }

    /// <summary>
    /// The QR code URI for easy setup in authenticator apps.
    /// Format: otpauth://totp/AppName:user@email.com?secret=KEY&issuer=AppName
    /// </summary>
    public string? AuthenticatorUri { get; set; }

    // ===== SMS DATA =====
    /// <summary>
    /// Phone number for SMS-based MFA (E.164 format recommended).
    /// Example: +1234567890
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates if the phone number has been verified.
    /// </summary>
    public bool IsPhoneVerified { get; set; }

    /// <summary>
    /// When the phone number was last verified.
    /// </summary>
    public DateTime? PhoneVerifiedAt { get; set; }

    // ===== EMAIL DATA =====
    /// <summary>
    /// Email address for email-based MFA.
    /// Can be different from the user's primary email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Indicates if the email has been verified.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// When the email was last verified.
    /// </summary>
    public DateTime? EmailVerifiedAt { get; set; }

    // ===== BACKUP/RECOVERY CODES =====
    /// <summary>
    /// Comma-separated hashed backup codes for emergency access.
    /// Each code can only be used once.
    /// Example: "hash1,hash2,hash3,hash4,hash5"
    /// </summary>
    public string? BackupCodesHash { get; set; }

    /// <summary>
    /// Comma-separated backup codes (alias for consistency).
    /// </summary>
    public string? BackupCodes
    {
        get => BackupCodesHash;
        set => BackupCodesHash = value;
    }

    /// <summary>
    /// Number of unused backup codes remaining.
    /// </summary>
    public int BackupCodesRemaining { get; set; }

    /// <summary>
    /// When the backup codes were last generated.
    /// </summary>
    public DateTime? BackupCodesGeneratedAt { get; set; }

    // ===== USAGE TRACKING =====
    /// <summary>
    /// When this MFA method was last used successfully.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Number of times this MFA method has been used.
    /// </summary>
    public int UseCount { get; set; }

    /// <summary>
    /// Number of failed verification attempts.
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// Verification code for SMS/Email MFA (hashed).
    /// </summary>
    public string? VerificationCode { get; set; }

    /// <summary>
    /// Expiry time for the verification code.
    /// </summary>
    public DateTime? VerificationCodeExpiry { get; set; }

    /// <summary>
    /// When the method is locked due to too many failed attempts.
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser User { get; set; } = null!;

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Checks if the method is currently locked due to failed attempts.
    /// </summary>
    public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// Checks if the method is ready to use (enabled, verified, and not locked).
    /// </summary>
    public bool IsReady => IsEnabled && IsVerified && !IsLocked;

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Marks the MFA method as successfully used.
    /// </summary>
    public void RecordSuccessfulUse()
    {
        LastUsedAt = DateTime.UtcNow;
        UseCount++;
        FailedAttempts = 0;
        LockedUntil = null;
        MarkAsUpdated();
    }

    /// <summary>
    /// Records a failed verification attempt and locks if threshold exceeded.
    /// </summary>
    public void RecordFailedAttempt(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedAttempts++;

        if (FailedAttempts >= maxAttempts)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Resets failed attempts and unlocks the method.
    /// </summary>
    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LockedUntil = null;
        MarkAsUpdated();
    }

    /// <summary>
    /// Verifies the method (phone or email).
    /// </summary>
    public void Verify()
    {
        IsVerified = true;

        if (MethodType == MfaMethodType.Sms && !string.IsNullOrEmpty(PhoneNumber))
        {
            IsPhoneVerified = true;
            PhoneVerifiedAt = DateTime.UtcNow;
        }
        else if (MethodType == MfaMethodType.Email && !string.IsNullOrEmpty(Email))
        {
            IsEmailVerified = true;
            EmailVerifiedAt = DateTime.UtcNow;
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Enables the MFA method.
    /// </summary>
    public void Enable()
    {
        if (!IsVerified)
        {
            throw new InvalidOperationException("Cannot enable MFA method before verification.");
        }

        IsEnabled = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Disables the MFA method.
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        IsPrimary = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Sets this method as the primary MFA method.
    /// </summary>
    public void SetAsPrimary()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Cannot set disabled method as primary.");
        }

        IsPrimary = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Removes primary status from this method.
    /// </summary>
    public void RemovePrimary()
    {
        IsPrimary = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Uses one backup code.
    /// </summary>
    public void UseBackupCode()
    {
        if (BackupCodesRemaining > 0)
        {
            BackupCodesRemaining--;
            MarkAsUpdated();
        }
    }

    /// <summary>
    /// Factory method to create an authenticator MFA method.
    /// </summary>
    public static MfaMethod CreateAuthenticator(
        Guid userId,
        string tenantId,
        string authenticatorKey,
        string authenticatorUri)
    {
        return new MfaMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            MethodType = MfaMethodType.Authenticator,
            AuthenticatorKey = authenticatorKey,
            AuthenticatorUri = authenticatorUri,
            IsEnabled = false,
            IsVerified = false,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create an SMS MFA method.
    /// </summary>
    public static MfaMethod CreateSms(
        Guid userId,
        string tenantId,
        string phoneNumber)
    {
        return new MfaMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            MethodType = MfaMethodType.Sms,
            PhoneNumber = phoneNumber,
            IsPhoneVerified = false,
            IsEnabled = false,
            IsVerified = false,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method to create an Email MFA method.
    /// </summary>
    public static MfaMethod CreateEmail(
        Guid userId,
        string tenantId,
        string email)
    {
        return new MfaMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            MethodType = MfaMethodType.Email,
            Email = email,
            IsEmailVerified = false,
            IsEnabled = false,
            IsVerified = false,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}
