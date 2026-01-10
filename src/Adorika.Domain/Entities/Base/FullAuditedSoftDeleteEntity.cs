namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Base entity class with full auditing, tenant support, and soft delete functionality.
/// Combines auditing (creation/update tracking), multi-tenant isolation, and soft delete pattern.
/// Use this for entities that should never be permanently deleted from the database.
/// </summary>
public abstract class FullAuditedSoftDeleteEntity : FullAuditedEntity, ISoftDeleteEntity
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the entity was soft-deleted (null if not deleted).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who deleted this entity (null if not deleted or system-deleted).
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Checks if the entity is active (not soft-deleted).
    /// </summary>
    public bool IsActive => !IsDeleted;

    /// <summary>
    /// Soft-deletes the entity by the specified user.
    /// </summary>
    /// <param name="userId">User ID who is deleting the entity.</param>
    public virtual void SoftDelete(Guid? userId = null)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Entity is already deleted.");
        }

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
        MarkAsUpdated(userId);
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    /// <param name="userId">User ID who is restoring the entity.</param>
    public virtual void Restore(Guid? userId = null)
    {
        if (!IsDeleted)
        {
            throw new InvalidOperationException("Entity is not deleted.");
        }

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        MarkAsUpdated(userId);
    }

    /// <summary>
    /// Soft-deletes the entity with tenant validation.
    /// </summary>
    /// <param name="tenantId">Tenant ID (for validation).</param>
    /// <param name="userId">User ID who is deleting the entity.</param>
    public void SoftDelete(string tenantId, Guid? userId = null)
    {
        ValidateTenantAccess(tenantId);
        SoftDelete(userId);
    }

    /// <summary>
    /// Restores the entity with tenant validation.
    /// </summary>
    /// <param name="tenantId">Tenant ID (for validation).</param>
    /// <param name="userId">User ID who is restoring the entity.</param>
    public void Restore(string tenantId, Guid? userId = null)
    {
        ValidateTenantAccess(tenantId);
        Restore(userId);
    }
}
