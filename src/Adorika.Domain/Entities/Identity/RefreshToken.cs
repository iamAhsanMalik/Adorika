namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a refresh token for JWT authentication.
/// Enables secure token refresh without requiring re-authentication.
/// </summary>
public class RefreshToken
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this refresh token belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== USER ASSOCIATION =====
    public Guid UserId { get; set; }

    // ===== TOKEN DATA =====
    /// <summary>
    /// The hashed refresh token value.
    /// Stored as a hash for security (never store tokens in plain text).
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional device identifier for tracking which device this token belongs to.
    /// Enables per-device token management.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Optional device information for display purposes.
    /// Example: "Chrome on Windows 11"
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP address from which the token was created.
    /// </summary>
    public string? IpAddress { get; set; }

    // ===== TOKEN STATUS =====
    /// <summary>
    /// Indicates if the token has been revoked (invalidated).
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// When the token was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for token revocation.
    /// Examples: "User logout", "Password changed", "Security alert", "Admin action"
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Alias for RevokedReason for consistency across the codebase.
    /// </summary>
    public string? RevocationReason
    {
        get => RevokedReason;
        set => RevokedReason = value;
    }

    /// <summary>
    /// IP address from which the token was revoked.
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Indicates if the token has been used to generate a new access token.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// When the token was used.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// The new refresh token that replaced this one (for token rotation).
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    // ===== TIMESTAMPS =====
    /// <summary>
    /// When the refresh token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser User { get; set; } = null!;

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Checks if the refresh token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if the refresh token is active (not revoked, not used, not expired).
    /// </summary>
    public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;

    /// <summary>
    /// Gets the remaining lifetime of the token.
    /// </summary>
    public TimeSpan? RemainingLifetime =>
        IsExpired ? null : ExpiresAt - DateTime.UtcNow;

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Revokes the refresh token with a specified reason.
    /// </summary>
    public void Revoke(string reason, string? ipAddress = null)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Token is already revoked.");
        }

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        RevokedByIp = ipAddress;
    }

    /// <summary>
    /// Marks the token as used and optionally sets the replacement token.
    /// </summary>
    public void MarkAsUsed(Guid? replacedByTokenId = null)
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Token has already been used.");
        }

        if (IsRevoked)
        {
            throw new InvalidOperationException("Cannot use a revoked token.");
        }

        if (IsExpired)
        {
            throw new InvalidOperationException("Cannot use an expired token.");
        }

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }

    /// <summary>
    /// Factory method to create a new refresh token.
    /// </summary>
    public static RefreshToken Create(
        Guid userId,
        string tenantId,
        string tokenHash,
        TimeSpan lifetime,
        string? deviceId = null,
        string? deviceInfo = null,
        string? ipAddress = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TokenHash = tokenHash,
            DeviceId = deviceId,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            IsRevoked = false,
            IsUsed = false
        };
    }
}
