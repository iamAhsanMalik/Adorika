namespace Adorika.Domain.Events;

/// <summary>
/// Domain event raised when the system completes initial installation.
/// This is a critical event that should trigger audit logging and notifications.
/// </summary>
public sealed record SystemInitializedEvent : DomainEvent
{
    /// <summary>
    /// The system configuration ID.
    /// </summary>
    public Guid SystemConfigurationId { get; init; }

    /// <summary>
    /// The ID of the initial tenant created during installation.
    /// </summary>
    public string InitialTenantId { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the super user created during installation.
    /// </summary>
    public Guid InitialSuperUserId { get; init; }

    /// <summary>
    /// The schema version installed.
    /// </summary>
    public string SchemaVersion { get; init; } = string.Empty;

    /// <summary>
    /// IP address from which the installation was initiated.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// User agent of the client that initiated the installation.
    /// </summary>
    public string? UserAgent { get; init; }

    public SystemInitializedEvent(
        Guid systemConfigurationId,
        string initialTenantId,
        Guid initialSuperUserId,
        string schemaVersion,
        string? ipAddress = null,
        string? userAgent = null)
    {
        SystemConfigurationId = systemConfigurationId;
        InitialTenantId = initialTenantId;
        InitialSuperUserId = initialSuperUserId;
        SchemaVersion = schemaVersion;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}
