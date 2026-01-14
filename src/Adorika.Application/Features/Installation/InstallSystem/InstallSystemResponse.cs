namespace Adorika.Application.Features.Installation.InstallSystem;

public record InstallSystemResponse
{
    public string? TenantId { get; init; }
    public string? TenantIdentifier { get; init; }
    public Guid? SuperUserId { get; init; }
    public string? SuperUserEmail { get; init; }
    public string? SchemaVersion { get; init; }

    /// <summary>
    /// The connection string formatted for Docker Environment Variables.
    /// Display this to the user so they can persist their configuration.
    /// </summary>
    public string? EnvironmentVariable { get; init; }

    public static InstallSystemResponse Create(
        string tenantId,
        string tenantIdentifier,
        Guid superUserId,
        string superUserEmail,
        string schemaVersion,
        string envVar) =>
        new()
        {
            TenantId = tenantId,
            TenantIdentifier = tenantIdentifier,
            SuperUserId = superUserId,
            SuperUserEmail = superUserEmail,
            SchemaVersion = schemaVersion,
            EnvironmentVariable = envVar
        };
}
