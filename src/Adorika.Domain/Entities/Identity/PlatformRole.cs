using Microsoft.AspNetCore.Identity;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a platform-level role for global system administration.
/// This entity is NOT tenant-scoped and belongs to PlatformDbContext.
/// Platform roles are used for SuperAdmins who manage the entire system.
/// </summary>
public class PlatformRole : IdentityRole<Guid>
{
    /// <summary>
    /// Description of the role's purpose and permissions.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if this role is active and can be assigned to users.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if this is a system-defined role that cannot be deleted.
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Timestamp when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the role was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // ===== NAVIGATION PROPERTIES =====
    public virtual ICollection<PlatformRolePermission> Permissions { get; set; } = new List<PlatformRolePermission>();

    // ===== FACTORY METHODS =====
    public static PlatformRole CreateSystemAdmin()
    {
        return new PlatformRole
        {
            Id = Guid.NewGuid(),
            Name = "SystemAdmin",
            NormalizedName = "SYSTEMADMIN",
            Description = "System Administrator with full platform access",
            IsActive = true,
            IsSystemRole = true,
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    public static PlatformRole Create(string name, string? description = null)
    {
        return new PlatformRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            IsActive = true,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    // ===== DOMAIN METHODS =====
    public void Deactivate()
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("Cannot deactivate a system role.");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}
