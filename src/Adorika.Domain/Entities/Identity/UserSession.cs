namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents an active user session for tracking and management.
/// Enables session-based security features like remote logout and device tracking.
/// </summary>
public class UserSession
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this session belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== USER ASSOCIATION =====
    public Guid UserId { get; set; }

    // ===== SESSION IDENTIFICATION =====
    /// <summary>
    /// Unique session token (hashed for security).
    /// Used to validate and track individual sessions.
    /// </summary>
    public string SessionToken { get; set; } = string.Empty;

    /// <summary>
    /// Alternative session identifier for easier lookup.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    // ===== DEVICE & CLIENT INFORMATION =====
    /// <summary>
    /// Human-readable device information.
    /// Example: "Chrome on Windows 11", "Safari on iPhone 14"
    /// </summary>
    public string DeviceInfo { get; set; } = string.Empty;

    /// <summary>
    /// Device identifier for tracking unique devices.
    /// Can be used for "Remember this device" functionality.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// IP address from which the session was created.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User agent string from the browser/client.
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Geographic location if available (city, country).
    /// Example: "New York, USA"
    /// </summary>
    public string? Location { get; set; }

    // ===== SESSION STATUS =====
    /// <summary>
    /// Indicates if the session is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if this is a "Remember Me" session with extended lifetime.
    /// </summary>
    public bool IsPersistent { get; set; }

    /// <summary>
    /// Indicates if the session is currently locked (lock screen feature).
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// When the session was locked.
    /// </summary>
    public DateTime? LockedAt { get; set; }

    // ===== TIMESTAMPS =====
    /// <summary>
    /// When the session was created (login time).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the session expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Last activity timestamp for idle timeout tracking.
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>
    /// When the session was revoked (manually or due to logout).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for session revocation.
    /// Examples: "User logout", "Admin revoked", "Password changed", "Security timeout"
    /// </summary>
    public string? RevocationReason { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser User { get; set; } = null!;

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Checks if the session has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if the session is revoked.
    /// </summary>
    public bool IsRevoked
    {
        get => RevokedAt.HasValue;
        set
        {
            if (value && !RevokedAt.HasValue)
            {
                RevokedAt = DateTime.UtcNow;
            }
            else if (!value)
            {
                RevokedAt = null;
            }
        }
    }

    /// <summary>
    /// Checks if the session is still valid (active and not expired).
    /// </summary>
    public bool IsValid => IsActive && !IsExpired && !IsRevoked;

    /// <summary>
    /// Gets the duration since last activity.
    /// </summary>
    public TimeSpan? TimeSinceLastActivity =>
        LastActivityAt.HasValue ? DateTime.UtcNow - LastActivityAt.Value : null;

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Updates the last activity timestamp to current time.
    /// </summary>
    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes the session with a specified reason.
    /// </summary>
    public void Revoke(string reason)
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason;
    }

    /// <summary>
    /// Extends the session expiration time.
    /// </summary>
    public void ExtendExpiration(TimeSpan extension)
    {
        ExpiresAt = DateTime.UtcNow.Add(extension);
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the session should be considered idle based on the provided timeout.
    /// </summary>
    public bool IsIdle(TimeSpan idleTimeout)
    {
        if (!LastActivityAt.HasValue)
        {
            return false;
        }

        return DateTime.UtcNow - LastActivityAt.Value > idleTimeout;
    }

    /// <summary>
    /// Factory method to create a new session.
    /// </summary>
    public static UserSession Create(
        Guid userId,
        string tenantId,
        string sessionToken,
        string deviceInfo,
        string ipAddress,
        string userAgent,
        bool isPersistent,
        TimeSpan expirationTime,
        string? deviceId = null,
        string? location = null)
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            SessionToken = sessionToken,
            SessionId = Guid.NewGuid().ToString("N"),
            DeviceInfo = deviceInfo,
            DeviceId = deviceId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Location = location,
            IsPersistent = isPersistent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expirationTime),
            LastActivityAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}
