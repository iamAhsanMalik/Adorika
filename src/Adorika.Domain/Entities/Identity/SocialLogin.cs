namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a linked social authentication provider for a user.
/// Enables login via Google, Facebook, Apple, and other OAuth providers.
/// </summary>
public class SocialLogin
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this social login belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== USER ASSOCIATION =====
    public Guid UserId { get; set; }

    // ===== PROVIDER INFORMATION =====
    /// <summary>
    /// The name of the social authentication provider.
    /// Examples: "Google", "Facebook", "Apple", "Microsoft", "GitHub"
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier from the external provider for this user.
    /// This is the user's ID in the provider's system (e.g., Google User ID).
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Display name returned by the provider.
    /// Example: "John Doe"
    /// </summary>
    public string? ProviderDisplayName { get; set; }

    /// <summary>
    /// Email address returned by the provider.
    /// May be different from the user's primary email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Profile picture URL from the provider.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    // ===== PROVIDER TOKENS =====
    /// <summary>
    /// Access token from the provider (encrypted in storage).
    /// Used to access provider APIs on behalf of the user.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh token from the provider (encrypted in storage).
    /// Used to obtain new access tokens without re-authentication.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the access token expires.
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    // ===== STATUS =====
    /// <summary>
    /// Indicates if this social login is active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the email from the provider has been verified.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    // ===== TIMESTAMPS =====
    /// <summary>
    /// When this social account was linked to the user.
    /// </summary>
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this social login was last used for authentication.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// When the social login was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ===== USAGE TRACKING =====
    /// <summary>
    /// Number of times this social login has been used.
    /// </summary>
    public int UseCount { get; set; }

    /// <summary>
    /// IP address from which the account was last used.
    /// </summary>
    public string? LastUsedIp { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ApplicationUser User { get; set; } = null!;

    // ===== COMPUTED PROPERTIES =====
    /// <summary>
    /// Checks if the access token has expired.
    /// </summary>
    public bool IsTokenExpired =>
        TokenExpiresAt.HasValue && TokenExpiresAt.Value <= DateTime.UtcNow;

    /// <summary>
    /// Gets the provider and key as a composite identifier.
    /// </summary>
    public string ProviderIdentifier => $"{Provider}:{ProviderKey}";

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Records a successful use of this social login.
    /// </summary>
    public void RecordUse(string? ipAddress = null)
    {
        LastUsedAt = DateTime.UtcNow;
        LastUsedIp = ipAddress;
        UseCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the provider tokens.
    /// </summary>
    public void UpdateTokens(
        string? accessToken,
        string? refreshToken = null,
        DateTime? expiresAt = null)
    {
        AccessToken = accessToken;

        if (refreshToken != null)
        {
            RefreshToken = refreshToken;
        }

        TokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the profile information from the provider.
    /// </summary>
    public void UpdateProfile(
        string? displayName = null,
        string? email = null,
        string? profilePictureUrl = null,
        bool? isEmailVerified = null)
    {
        if (displayName != null)
        {
            ProviderDisplayName = displayName;
        }

        if (email != null)
        {
            Email = email;
        }

        if (profilePictureUrl != null)
        {
            ProfilePictureUrl = profilePictureUrl;
        }

        if (isEmailVerified.HasValue)
        {
            IsEmailVerified = isEmailVerified.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates this social login.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reactivates this social login.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new social login.
    /// </summary>
    public static SocialLogin Create(
        Guid userId,
        string tenantId,
        string provider,
        string providerKey,
        string? email = null,
        string? displayName = null,
        string? profilePictureUrl = null,
        bool isEmailVerified = false,
        string? accessToken = null,
        string? refreshToken = null,
        DateTime? tokenExpiresAt = null)
    {
        return new SocialLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            Provider = provider,
            ProviderKey = providerKey,
            Email = email,
            ProviderDisplayName = displayName,
            ProfilePictureUrl = profilePictureUrl,
            IsEmailVerified = isEmailVerified,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = tokenExpiresAt,
            LinkedAt = DateTime.UtcNow,
            IsActive = true,
            UseCount = 0
        };
    }
}
