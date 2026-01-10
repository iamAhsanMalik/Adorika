using Adorika.Domain.Entities.Base;

namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents a group of users with shared permissions.
/// Groups provide an additional layer of permission aggregation beyond roles.
/// </summary>
public class Group : FullAuditedEntity
{

    // ===== GROUP METADATA =====
    /// <summary>
    /// The name of the group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The normalized name for case-insensitive lookups.
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Description of the group's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // ===== HIERARCHY =====
    /// <summary>
    /// Parent group ID for hierarchical group structures.
    /// Null for top-level groups.
    /// </summary>
    public Guid? ParentGroupId { get; set; }

    /// <summary>
    /// Navigation property to parent group.
    /// </summary>
    public virtual Group? ParentGroup { get; set; }

    /// <summary>
    /// Navigation property to child groups.
    /// </summary>
    public virtual ICollection<Group> ChildGroups { get; set; } = new List<Group>();

    // ===== STATUS =====
    /// <summary>
    /// Indicates if this group is active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// System groups cannot be deleted or modified by users.
    /// Examples: Administrators, AllUsers
    /// </summary>
    public bool IsSystemGroup { get; set; }



    // ===== NAVIGATION PROPERTIES =====
    /// <summary>
    /// Users that belong to this group.
    /// </summary>
    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    /// <summary>
    /// Permissions assigned to this group.
    /// </summary>
    public virtual ICollection<GroupPermission> Permissions { get; set; } = new List<GroupPermission>();

    // ===== FACTORY METHODS =====
    public static Group Create(
        string tenantId,
        string name,
        string description,
        Guid? parentGroupId = null,
        Guid? createdByUserId = null)
    {
        return new Group
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            ParentGroupId = parentGroupId,
            IsSystemGroup = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdByUserId
        };
    }

    // ===== DOMAIN METHODS =====
    public void Update(string? name = null, string? description = null, Guid? userId = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Name = name;
            NormalizedName = name.ToUpperInvariant();
        }

        if (description != null)
        {
            Description = description;
        }

        MarkAsUpdated(userId);
    }

    public void Deactivate(Guid? userId = null)
    {
        IsActive = false;
        MarkAsUpdated(userId);
    }

    public void Activate(Guid? userId = null)
    {
        IsActive = true;
        MarkAsUpdated(userId);
    }

    public void SetParentGroup(Guid? parentGroupId, Guid? userId = null)
    {
        ParentGroupId = parentGroupId;
        MarkAsUpdated(userId);
    }

    /// <summary>
    /// Checks if this group is a descendant of another group (for preventing circular references).
    /// </summary>
    public bool IsDescendantOf(Guid groupId)
    {
        var current = ParentGroup;
        var visited = new HashSet<Guid> { Id };

        while (current != null)
        {
            if (current.Id == groupId)
            {
                return true;
            }

            // Prevent infinite loops from circular references
            if (!visited.Add(current.Id))
            {
                break;
            }

            current = current.ParentGroup;
        }

        return false;
    }
}
