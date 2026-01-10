using Adorika.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a user in the system with multi-tenant support.
/// Extends IdentityUser to add tenant isolation and additional security features.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this user belongs to.
    /// NULL for platform-level users (SystemAdmin).
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string? TenantId { get; set; }

    // ===== PROFILE INFORMATION =====
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    // ===== STATUS & TRACKING =====
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    // ===== MULTI-FACTOR AUTHENTICATION =====
    public bool IsMfaEnabled { get; set; }

    /// <summary>
    /// Flags indicating which MFA methods are enabled for this user.
    /// A user can have multiple methods enabled (e.g., Authenticator | Sms).
    /// </summary>
    public MfaMethodType EnabledMfaMethods { get; set; } = MfaMethodType.None;

    /// <summary>
    /// The preferred MFA method for login when multiple methods are enabled.
    /// User chooses this during 2FA login flow.
    /// </summary>
    public MfaMethodType? PreferredMfaMethod { get; set; }

    // ===== LOCK SCREEN =====
    public bool IsScreenLocked { get; set; }
    public DateTime? ScreenLockedAt { get; set; }

    // ===== ENHANCED SECURITY =====
    public int FailedLoginAttempts { get; set; }
    public int FailedUnlockAttempts { get; set; }
    public DateTime? AccountLockedUntil { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public bool MustChangePassword { get; set; }
    public string? BiometricPublicKey { get; set; }

    // ===== WORKING DAYS & TIME-OFF =====
    /// <summary>
    /// Flags indicating which days of the week the user is allowed to work/login.
    /// Example: Monday | Tuesday | Wednesday | Thursday | Friday for a typical work week.
    /// </summary>
    public WorkingDay WorkingDays { get; set; } = WorkingDay.Monday | WorkingDay.Tuesday |
                                                   WorkingDay.Wednesday | WorkingDay.Thursday |
                                                   WorkingDay.Friday;

    /// <summary>
    /// Indicates if working day restrictions are enforced for this user.
    /// If false, user can login any day regardless of WorkingDays setting.
    /// </summary>
    public bool EnforceWorkingDays { get; set; } = false;

    // ===== NAVIGATION PROPERTIES =====
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public virtual ICollection<MfaMethod> MfaMethods { get; set; } = new List<MfaMethod>();
    public virtual ICollection<SocialLogin> SocialLogins { get; set; } = new List<SocialLogin>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public virtual ICollection<UserTimeOff> TimeOffs { get; set; } = new List<UserTimeOff>();

    // ===== COMPUTED PROPERTIES =====
    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsAccountLocked =>
        AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow;

    /// <summary>
    /// Checks if the user has multiple MFA methods enabled.
    /// </summary>
    public bool HasMultipleMfaMethods
    {
        get
        {
            if (EnabledMfaMethods == MfaMethodType.None)
            {
                return false;
            }
            // Check if more than one bit is set
            var value = (int) EnabledMfaMethods;
            return (value & (value - 1)) != 0;
        }
    }

    /// <summary>
    /// Checks if the user is currently on time-off (holiday, vacation, etc.).
    /// </summary>
    public bool IsOnTimeOff
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            return TimeOffs.Any(t => t.IsActive && t.IsApproved &&
                                    today >= t.StartDate.Date && today <= t.EndDate.Date);
        }
    }

    // ===== FACTORY METHODS =====
    public static ApplicationUser Create(
        string? tenantId,
        string userName,
        string email,
        string firstName,
        string lastName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            IsActive = true,
            IsMfaEnabled = false,
            IsScreenLocked = false,
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    /// <summary>
    /// Creates a platform-level user (SystemAdmin) with NULL TenantId.
    /// </summary>
    public static ApplicationUser CreatePlatformUser(
        string userName,
        string email,
        string firstName,
        string lastName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = null, // Platform-level user
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = false,
            IsActive = true,
            IsMfaEnabled = false,
            IsScreenLocked = false,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    // ===== DOMAIN METHODS =====
    public void UpdateLastLogin(string? ipAddress = null)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFailedLoginAttempts(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            AccountLockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void LockScreen()
    {
        IsScreenLocked = true;
        ScreenLockedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnlockScreen()
    {
        IsScreenLocked = false;
        ScreenLockedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableMfa(MfaMethodType method)
    {
        IsMfaEnabled = true;
        EnabledMfaMethods |= method; // Add to enabled methods using bitwise OR

        // Set as preferred if it's the only method
        if (PreferredMfaMethod == null || !HasMultipleMfaMethods)
        {
            PreferredMfaMethod = method;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableMfa(MfaMethodType? method = null)
    {
        if (method.HasValue)
        {
            // Disable specific method
            EnabledMfaMethods &= ~method.Value; // Remove using bitwise AND with NOT

            // If disabled method was preferred, clear preference
            if (PreferredMfaMethod == method.Value)
            {
                PreferredMfaMethod = null;
            }

            // If no methods left, disable MFA entirely
            if (EnabledMfaMethods == MfaMethodType.None)
            {
                IsMfaEnabled = false;
                PreferredMfaMethod = null;
            }
        }
        else
        {
            // Disable all MFA
            IsMfaEnabled = false;
            EnabledMfaMethods = MfaMethodType.None;
            PreferredMfaMethod = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPreferredMfaMethod(MfaMethodType method)
    {
        // Verify the method is enabled
        if (!IsMfaEnabled || (EnabledMfaMethods & method) == 0)
        {
            throw new InvalidOperationException(
                $"Cannot set {method} as preferred method because it's not enabled.");
        }

        PreferredMfaMethod = method;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasMfaMethodEnabled(MfaMethodType method)
    {
        return IsMfaEnabled && (EnabledMfaMethods & method) != 0;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword()
    {
        PasswordChangedAt = DateTime.UtcNow;
        MustChangePassword = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RequirePasswordChange()
    {
        MustChangePassword = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ===== WORKING DAYS METHODS =====
    public void SetWorkingDays(WorkingDay days, bool enforce = true)
    {
        WorkingDays = days;
        EnforceWorkingDays = enforce;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanLoginOnDay(DayOfWeek day)
    {
        if (!EnforceWorkingDays)
        {
            return true;
        }

        var workingDayFlag = day switch
        {
            DayOfWeek.Monday => WorkingDay.Monday,
            DayOfWeek.Tuesday => WorkingDay.Tuesday,
            DayOfWeek.Wednesday => WorkingDay.Wednesday,
            DayOfWeek.Thursday => WorkingDay.Thursday,
            DayOfWeek.Friday => WorkingDay.Friday,
            DayOfWeek.Saturday => WorkingDay.Saturday,
            DayOfWeek.Sunday => WorkingDay.Sunday,
            _ => WorkingDay.None
        };

        return (WorkingDays & workingDayFlag) != 0;
    }

    public bool CanLoginToday()
    {
        var today = DateTime.UtcNow;

        // Check working day
        if (!CanLoginOnDay(today.DayOfWeek))
        {
            return false;
        }

        // Check time-off
        if (IsOnTimeOff)
        {
            return false;
        }

        return true;
    }

    public bool IsOnTimeOffOn(DateTime date)
    {
        var dateOnly = date.Date;
        return TimeOffs.Any(t => t.IsActive && t.IsApproved &&
                               dateOnly >= t.StartDate.Date && dateOnly <= t.EndDate.Date);
    }
}
