namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Interface for entities that track creation and modification timestamps.
/// Implements audit trail for when entities are created and updated.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// When the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the entity was last updated (null if never updated).
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who created this entity (null for system-created).
    /// </summary>
    Guid? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this entity.
    /// </summary>
    Guid? UpdatedBy { get; set; }
}
