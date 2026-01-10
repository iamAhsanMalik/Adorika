namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a permission assigned to a platform-level role.
/// This entity is NOT tenant-scoped and belongs to PlatformDbContext.
/// Platform permissions control access to system-wide operations like tenant management.
/// </summary>
public class PlatformRolePermission
{
    /// <summary>
    /// Unique identifier for this permission.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The role this permission belongs to.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// The resource this permission applies to (e.g., "Tenants", "System", "Users").
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// The action allowed on the resource (e.g., "Create", "Read", "Update", "Delete", "Manage").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Optional scope or conditions for this permission (e.g., "All", "Own", "Specific").
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Optional description of what this permission grants.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if this permission is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the permission was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the permission was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual PlatformRole Role { get; set; } = null!;

    // ===== FACTORY METHODS =====
    public static PlatformRolePermission Create(
        Guid roleId,
        string resource,
        string action,
        string? scope = null,
        string? description = null)
    {
        return new PlatformRolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            Resource = resource,
            Action = action,
            Scope = scope,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PlatformRolePermission CreateTenantManagement(Guid roleId)
    {
        return Create(
            roleId,
            "Tenants",
            "Manage",
            "All",
            "Full access to manage all tenants");
    }

    public static PlatformRolePermission CreateSystemConfiguration(Guid roleId)
    {
        return Create(
            roleId,
            "System",
            "Configure",
            "All",
            "Full access to system configuration");
    }

    public static PlatformRolePermission CreatePlatformUsers(Guid roleId)
    {
        return Create(
            roleId,
            "PlatformUsers",
            "Manage",
            "All",
            "Full access to manage platform users");
    }

    // ===== DOMAIN METHODS =====
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

    public void UpdateScope(string? scope)
    {
        Scope = scope;
        UpdatedAt = DateTime.UtcNow;
    }
}
