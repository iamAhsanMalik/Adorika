namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a password reset token for the forgot password flow.
/// Tokens are time-limited and single-use for security.
/// </summary>
public class PasswordResetToken
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this token belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== USER ASSOCIATION =====
    public Guid UserId { get; set; }

    // ===== TOKEN DATA =====
    /// <summary>
    /// The hashed token value.
    /// Never store tokens in plain text - always hash them.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional token identifier for lookup (not the actual token).
    /// Can be used as a public reference to the token.
    /// </summary>
    public string? TokenIdentifier { get; set; }

    // ===== STATUS =====
    /// <summary>
    /// Indicates if the token has been used to reset the password.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// When the token was used.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// IP address from which the token was used.
    /// </summary>
    public string? UsedFromIp { get; set; }

    /// <summary>
    /// Indicates if the token has been manually invalidated.
    /// </summary>
    public bool IsInvalidated { get; set; }

    /// <summary>
    /// When the token was invalidated.
    /// </summary>
    public DateTime? InvalidatedAt { get; set; }

    /// <summary>
    /// Reason for invalidation.
    /// Examples: "New token requested", "Admin action", "Suspicious activity"
    /// </summary>
    public string? InvalidationReason { get; set; }

    // ===== TIMESTAMPS =====
    /// <summary>
    /// When the token was created (password reset requested).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the token expires.
    /// Typically 15-60 minutes from creation.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    // ===== REQUEST METADATA =====
    /// <summary>
    /// IP address from which the reset was requested.
    /// Used for security auditing and fraud detection.
    /// </summary>
    public string RequestedFromIp { get; set; } = string.Empty;

    /// <summary>
    /// User agent from the reset request.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Email address where the reset link was sent.
    /// Useful for audit trail.
    /// </summary>
    public string SentToEmail { get; set; } = string.Empty;

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser User { get; set; } = null!;

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Checks if the token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if the token is valid (not used, not invalidated, not expired).
    /// </summary>
    public bool IsValid => !IsUsed && !IsInvalidated && !IsExpired;

    /// <summary>
    /// Gets the remaining lifetime of the token.
    /// </summary>
    public TimeSpan? RemainingLifetime =>
        IsExpired ? null : ExpiresAt - DateTime.UtcNow;

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Marks the token as used.
    /// </summary>
    public void MarkAsUsed(string? ipAddress = null)
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("Token has already been used.");
        }

        if (IsInvalidated)
        {
            throw new InvalidOperationException("Cannot use an invalidated token.");
        }

        if (IsExpired)
        {
            throw new InvalidOperationException("Token has expired.");
        }

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UsedFromIp = ipAddress;
    }

    /// <summary>
    /// Invalidates the token with a reason.
    /// </summary>
    public void Invalidate(string reason)
    {
        if (IsInvalidated)
        {
            throw new InvalidOperationException("Token is already invalidated.");
        }

        IsInvalidated = true;
        InvalidatedAt = DateTime.UtcNow;
        InvalidationReason = reason;
    }

    /// <summary>
    /// Validates that the token can be used.
    /// </summary>
    public void ValidateCanBeUsed()
    {
        if (IsUsed)
        {
            throw new InvalidOperationException("This password reset link has already been used.");
        }

        if (IsInvalidated)
        {
            throw new InvalidOperationException($"This password reset link is no longer valid. {InvalidationReason}");
        }

        if (IsExpired)
        {
            throw new InvalidOperationException("This password reset link has expired. Please request a new one.");
        }
    }

    /// <summary>
    /// Factory method to create a new password reset token.
    /// </summary>
    public static PasswordResetToken Create(
        Guid userId,
        string tenantId,
        string tokenHash,
        string sentToEmail,
        string requestedFromIp,
        TimeSpan lifetime,
        string? userAgent = null,
        string? tokenIdentifier = null)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TokenHash = tokenHash,
            TokenIdentifier = tokenIdentifier,
            SentToEmail = sentToEmail,
            RequestedFromIp = requestedFromIp,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            IsUsed = false,
            IsInvalidated = false
        };
    }

    /// <summary>
    /// Creates a password reset token with default 1-hour expiration.
    /// </summary>
    public static PasswordResetToken Create(
        Guid userId,
        string tenantId,
        string tokenHash,
        string sentToEmail,
        string requestedFromIp,
        string? userAgent = null)
    {
        return Create(
            userId,
            tenantId,
            tokenHash,
            sentToEmail,
            requestedFromIp,
            TimeSpan.FromHours(1),
            userAgent,
            Guid.NewGuid().ToString("N"));
    }
}
