using Finbuckle.MultiTenant.Abstractions;

namespace Adorika.Domain.Entities.MultiTenancy;

/// <summary>
/// Extended tenant information with additional properties for multi-tenant management.
/// Inherits from Finbuckle.MultiTenant's TenantInfo base record.
/// Uses mutable properties to support EF Core change tracking.
/// </summary>
public record AppTenantInfo : TenantInfo
{
    /// <summary>
    /// Database connection string for the tenant (optional, can use shared database).
    /// Shadows the base ConnectionString property to make it mutable.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Primary contact email for the tenant.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Whether the tenant is active (not soft-deleted).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the tenant is temporarily suspended.
    /// </summary>
    public bool IsSuspended { get; set; }

    /// <summary>
    /// Reason for tenant suspension (if suspended).
    /// </summary>
    public string? SuspensionReason { get; set; }

    /// <summary>
    /// When the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the tenant was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the tenant can be accessed.
    /// A tenant can be accessed only if it's active and not suspended.
    /// </summary>
    public bool CanAccess => IsActive && !IsSuspended;


    /// <summary>
    /// Constructor with required parameters.
    /// </summary>
    /// <param name="id">Unique tenant identifier (GUID)</param>
    /// <param name="identifier">Tenant identifier/slug (subdomain)</param>
    /// <param name="name">Tenant display name</param>
    public AppTenantInfo(string id, string identifier, string name)
        : base(id, identifier, name)
    {
    }

    /// <summary>
    /// Static factory method to create a new tenant with normalized identifier.
    /// </summary>
    /// <param name="identifier">Tenant identifier (will be normalized to lowercase)</param>
    /// <param name="name">Tenant display name</param>
    /// <param name="contactEmail">Optional contact email</param>
    /// <param name="connectionString">Optional dedicated connection string</param>
    /// <returns>New AppTenantInfo instance</returns>
    public static AppTenantInfo Create(
        string identifier,
        string name,
        string? contactEmail = null,
        string? connectionString = null)
    {
        var normalized = identifier.ToLowerInvariant();
        var tenant = new AppTenantInfo(Guid.NewGuid().ToString(), normalized, name)
        {
            ContactEmail = contactEmail,
            ConnectionString = connectionString,
            IsActive = true,
            IsSuspended = false,
            CreatedAt = DateTime.UtcNow
        };
        return tenant;
    }
}
