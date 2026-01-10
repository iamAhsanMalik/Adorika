using Microsoft.AspNetCore.Identity;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a platform-level user (SuperAdmin) with global system access.
/// This entity is NOT tenant-scoped and belongs to PlatformDbContext.
/// Platform users can manage tenants, impersonate tenant contexts, and perform system-wide operations.
/// </summary>
public class PlatformUser : IdentityUser<Guid>
{
    // ===== PROFILE INFORMATION =====
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    // ===== STATUS & TRACKING =====
    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }

    // ===== MULTI-FACTOR AUTHENTICATION =====
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; }

    // ===== ENHANCED SECURITY =====
    public int FailedLoginAttempts { get; set; }
    public DateTime? AccountLockedUntil { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public bool MustChangePassword { get; set; }

    // ===== COMPUTED PROPERTIES =====
    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool IsAccountLocked =>
        AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow;

    // ===== FACTORY METHODS =====
    public static PlatformUser CreateSuperAdmin(
        string userName,
        string email,
        string firstName,
        string lastName)
    {
        return new PlatformUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = false,
            IsActive = true,
            IsSuperAdmin = true,
            IsMfaEnabled = false,
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

    public void IncrementFailedLoginAttempts(int maxAttempts = 5, int lockoutMinutes = 30)
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

    public void EnableMfa(string secret)
    {
        IsMfaEnabled = true;
        MfaSecret = secret;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableMfa()
    {
        IsMfaEnabled = false;
        MfaSecret = null;
        UpdatedAt = DateTime.UtcNow;
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
}
