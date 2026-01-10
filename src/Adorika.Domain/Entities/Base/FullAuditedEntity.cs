namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Base entity class with full auditing and tenant support.
/// Combines auditing (creation/update tracking) with multi-tenant isolation.
/// Use this for most domain entities in the system.
/// </summary>
public abstract class FullAuditedEntity : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The tenant identifier this entity belongs to.
    /// Critical for multi-tenant data isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Checks if the entity belongs to the specified tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID to check against.</param>
    /// <returns>True if the entity belongs to the tenant, false otherwise.</returns>
    public bool BelongsToTenant(string tenantId)
    {
        return TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates that the entity belongs to the specified tenant.
    /// Throws an exception if the tenant doesn't match.
    /// </summary>
    /// <param name="tenantId">Expected tenant ID.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when tenant IDs don't match.</exception>
    public void ValidateTenantAccess(string tenantId)
    {
        if (!BelongsToTenant(tenantId))
        {
            throw new UnauthorizedAccessException(
                $"Entity belongs to tenant '{TenantId}' but access was attempted from tenant '{tenantId}'.");
        }
    }

    /// <summary>
    /// Updates the entity and records the modification metadata.
    /// </summary>
    /// <param name="tenantId">Tenant ID (for validation).</param>
    /// <param name="userId">User ID who is updating the entity.</param>
    protected void Update(string tenantId, Guid? userId = null)
    {
        ValidateTenantAccess(tenantId);
        MarkAsUpdated(userId);
    }
}
