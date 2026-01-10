namespace Adorika.Domain.Entities.Base;

/// <summary>
/// Interface for entities that belong to a specific tenant in a multi-tenant system.
/// Ensures proper tenant isolation across all tenant-scoped entities.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The tenant identifier this entity belongs to.
    /// Critical for multi-tenant data isolation.
    /// NULL for platform-level entities (e.g., SystemAdmin users).
    /// </summary>
    string TenantId { get; set; }
}
