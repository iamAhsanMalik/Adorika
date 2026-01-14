using Adorika.Application.Common.Wrapper;
using Mediator;

namespace Adorika.Application.Features.Installation.InstallSystem;

/// <summary>
/// Request model for system installation.
/// Contains all required information to bootstrap the system.
/// </summary>
public record InstallSystemCommand : ICommand<Result<InstallSystemResponse>>
{
    /// <summary>
    /// Name of the initial tenant to create.
    /// </summary>
    public string TenantName { get; init; } = string.Empty;

    /// <summary>
    /// Identifier for the tenant (used in routes/headers). Will be normalized to lowercase.
    /// </summary>
    public string TenantIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Contact email for the tenant.
    /// </summary>
    public string? TenantContactEmail { get; init; }

    /// <summary>
    /// First name of the super user.
    /// </summary>
    public string SuperUserFirstName { get; init; } = string.Empty;

    /// <summary>
    /// Last name of the super user.
    /// </summary>
    public string SuperUserLastName { get; init; } = string.Empty;

    /// <summary>
    /// Email address for the super user (also used as username).
    /// </summary>
    public string SuperUserEmail { get; init; } = string.Empty;

    /// <summary>
    /// Password for the super user.
    /// </summary>
    public string SuperUserPassword { get; init; } = string.Empty;

    /// <summary>
    /// Database host (e.g., localhost, 127.0.0.1).
    /// </summary>
    public string DatabaseHost { get; init; } = "localhost";

    /// <summary>
    /// Database port (default: 5432 for PostgreSQL).
    /// </summary>
    public int DatabasePort { get; init; } = 5432;

    /// <summary>
    /// Name of the database to create/use.
    /// </summary>
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>
    /// Database username (must have permission to create databases).
    /// </summary>
    public string DatabaseUsername { get; init; } = "postgres";

    /// <summary>
    /// Database password.
    /// </summary>
    public string DatabasePassword { get; init; } = string.Empty;
}
