using Microsoft.AspNetCore.Identity;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a role in the system with multi-tenant support.
/// Extends IdentityRole to add tenant isolation and permission management.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this role belongs to.
    /// NULL for platform-level roles (SystemAdmin).
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string? TenantId { get; set; }

    // ===== ROLE METADATA =====
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// System roles cannot be deleted or modified by users.
    /// Examples: SuperAdmin, TenantAdmin
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Indicates if this role is active and can be assigned.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ===== TRACKING =====
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

    // ===== FACTORY METHODS =====
    public static ApplicationRole Create(string tenantId, string name, string description)
    {
        return new ApplicationRole
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            IsSystemRole = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
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

    public void UpdateDescription(string description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
