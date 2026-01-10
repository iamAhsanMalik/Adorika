namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Base entity class with auditing support.
/// Tracks creation and modification timestamps and user IDs.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    /// <summary>
    /// When the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated (null if never updated).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who created this entity (null for system-created).
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// User ID who last updated this entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Marks the entity as updated by the specified user.
    /// </summary>
    /// <param name="userId">User ID who is updating the entity.</param>
    protected void MarkAsUpdated(Guid? userId = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    /// <summary>
    /// Sets the creation metadata.
    /// </summary>
    /// <param name="userId">User ID who created the entity.</param>
    protected void SetCreatedBy(Guid? userId)
    {
        CreatedBy = userId;
        CreatedAt = DateTime.UtcNow;
    }
}
