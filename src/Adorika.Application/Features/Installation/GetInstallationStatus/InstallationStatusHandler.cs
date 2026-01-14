using Adorika.Application.Common.Interfaces.Services;
using Adorika.Application.Common.Wrapper;

using Mediator;

namespace Adorika.Application.Features.Installation.GetInstallationStatus;

/// <summary>
/// Handler for retrieving the current system installation status.
/// </summary>
public sealed class InstallationStatusHandler(ISystemInstallation systemInstallation)
    : IQueryHandler<InstallationStatusQuery, Result<InstallationStatusResponse>>
{
    public async ValueTask<Result<InstallationStatusResponse>> Handle(InstallationStatusQuery query, CancellationToken ct)
    {
        // 1. Fetch status from infrastructure service
        var result = await systemInstallation.GetInstallationStatus(ct);

        return result.Data is { IsInitialized: false }
            ? result.WithMessage("Installation required.")
                .AppendWarning("Detected uninitialized system database.")
            : result;
    }
}
