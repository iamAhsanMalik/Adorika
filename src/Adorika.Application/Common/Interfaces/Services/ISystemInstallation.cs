
using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;
using Adorika.Application.Features.Installation.GetInstallationStatus;
using Adorika.Application.Features.Installation.InstallSystem;

namespace Adorika.Infrastructure.Services;

public interface ISystemInstallation
{
    Task<Result<InstallationStatusResponse>> GetInstallationStatus(CancellationToken ct = default);
    Task<Result<InstallSystemResponse>> InstallSystem(InstallSystemCommand request, CancellationToken ct = default);
    Task<bool> TestDatabaseConnection(DbConnectionDto db, CancellationToken ct);
}
