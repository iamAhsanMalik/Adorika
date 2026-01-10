namespace Adorika.Domain.Entities.Identity;

/// <summary>
/// Represents the many-to-many relationship between users and groups.
/// Tracks group membership with additional metadata.
/// </summary>
public class UserGroup
{
    // ===== PRIMARY KEY =====
    public Guid Id { get; set; }

    // ===== TENANT ISOLATION =====
    /// <summary>
    /// The tenant identifier this membership belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    // ===== RELATIONSHIP =====
    /// <summary>
    /// The user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// The group ID.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Navigation property to the group.
    /// </summary>
    public virtual Group Group { get; set; } = null!;

    // ===== MEMBERSHIP METADATA =====
    /// <summary>
    /// Indicates if this membership is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional: When this membership becomes effective.
    /// Null means effective immediately.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Optional: When this membership expires.
    /// Null means no expiration.
    /// </summary>
    public DateTime? EffectiveUntil { get; set; }

    // ===== TRACKING =====
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? AddedByUserId { get; set; }
    public Guid? RemovedByUserId { get; set; }
    public DateTime? RemovedAt { get; set; }

    // ===== FACTORY METHODS =====
    public static UserGroup Create(
        string tenantId,
        Guid userId,
        Guid groupId,
        Guid? addedByUserId = null,
        DateTime? effectiveFrom = null,
        DateTime? effectiveUntil = null)
    {
        return new UserGroup
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            GroupId = groupId,
            IsActive = true,
            EffectiveFrom = effectiveFrom,
            EffectiveUntil = effectiveUntil,
            CreatedAt = DateTime.UtcNow,
            AddedByUserId = addedByUserId
        };
    }

    // ===== DOMAIN METHODS =====
    /// <summary>
    /// Checks if the membership is currently effective based on dates.
    /// </summary>
    public bool IsCurrentlyEffective()
    {
        if (!IsActive)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        if (EffectiveFrom.HasValue && now < EffectiveFrom.Value)
        {
            return false;
        }

        if (EffectiveUntil.HasValue && now > EffectiveUntil.Value)
        {
            return false;
        }

        return true;
    }

    public void Deactivate(Guid? removedByUserId = null)
    {
        IsActive = false;
        RemovedAt = DateTime.UtcNow;
        RemovedByUserId = removedByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        RemovedAt = null;
        RemovedByUserId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEffectiveDates(DateTime? effectiveFrom, DateTime? effectiveUntil)
    {
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        UpdatedAt = DateTime.UtcNow;
    }
}
