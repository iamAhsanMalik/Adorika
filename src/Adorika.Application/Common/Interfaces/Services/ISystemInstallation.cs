using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;
using Adorika.Application.Features.Installation.GetInstallationStatus;
using Adorika.Application.Features.Installation.InstallSystem;

namespace Adorika.Application.Common.Interfaces.Services;

public interface ISystemInstallation
{
    Task<Result<InstallationStatusResponse>> GetInstallationStatus(CancellationToken ct = default);
    Task<Result<InstallSystemResponse>> InstallSystem(InstallSystemCommand request, string? ipAddress = null, string? userAgent = null, CancellationToken ct = default);
    Task<bool> TestDatabaseConnection(DbConnectionDto db, CancellationToken ct);
}
