namespace Adorika.Application.Features.Installation.InstallSystem;

public record InstallSystemResponse
{
    public string? TenantId { get; init; }
    public string? TenantIdentifier { get; init; }
    public Guid? SuperUserId { get; init; }
    public string? SuperUserEmail { get; init; }
    public string? SchemaVersion { get; init; }

    /// <summary>
    /// Indicates whether the database has been configured.
    /// The connection string should be configured separately in environment variables.
    /// </summary>
    public bool DatabaseConfigured { get; init; }

    public static InstallSystemResponse Create(
        string tenantId,
        string tenantIdentifier,
        Guid superUserId,
        string superUserEmail,
        string schemaVersion) =>
        new()
        {
            TenantId = tenantId,
            TenantIdentifier = tenantIdentifier,
            SuperUserId = superUserId,
            SuperUserEmail = superUserEmail,
            SchemaVersion = schemaVersion,
            DatabaseConfigured = true
        };
}
