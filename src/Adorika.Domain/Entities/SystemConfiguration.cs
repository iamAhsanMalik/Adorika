namespace Adorika.Domain.Entities;

/// <summary>
/// Represents system-wide configuration and installation state.
/// This is NOT multi-tenant - it's a singleton entity for the entire system.
/// </summary>
public class SystemConfiguration
{
    public Guid Id { get; set; }

    /// <summary>
    /// Indicates whether the system has completed initial installation.
    /// Once true, this can NEVER be changed back to false.
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    /// Timestamp when the system was initialized.
    /// </summary>
    public DateTime? InitializedAt { get; set; }

    /// <summary>
    /// The initial tenant ID that was created during installation.
    /// </summary>
    public string? InitialTenantId { get; set; }

    /// <summary>
    /// The initial super user ID that was created during installation.
    /// </summary>
    public Guid? InitialSuperUserId { get; set; }

    /// <summary>
    /// Database schema version for tracking migrations.
    /// </summary>
    public string? SchemaVersion { get; set; }

    /// <summary>
    /// Last time the configuration was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Creates a new uninitialized system configuration.
    /// </summary>
    public static SystemConfiguration CreateUninitialized()
    {
        return new SystemConfiguration
        {
            Id = Guid.NewGuid(),
            IsInitialized = false,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks the system as initialized. This is irreversible.
    /// </summary>
    public void MarkAsInitialized(string tenantId, Guid superUserId, string schemaVersion)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("System is already initialized. Cannot re-initialize.");
        }

        IsInitialized = true;
        InitializedAt = DateTime.UtcNow;
        InitialTenantId = tenantId;
        InitialSuperUserId = superUserId;
        SchemaVersion = schemaVersion;
        UpdatedAt = DateTime.UtcNow;
    }
}
