namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Interface for entities that support soft delete (logical deletion).
/// Soft-deleted entities are marked as deleted but remain in the database.
/// </summary>
public interface ISoftDeleteEntity
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// When the entity was soft-deleted (null if not deleted).
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who deleted this entity (null if not deleted or system-deleted).
    /// </summary>
    Guid? DeletedBy { get; set; }
}
