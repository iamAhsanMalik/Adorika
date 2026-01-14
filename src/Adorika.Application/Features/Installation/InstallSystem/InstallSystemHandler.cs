using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;
using Adorika.Application.Features.Installation.InstallSystem;
using Adorika.Infrastructure.Services;

using Mediator;

public sealed class InstallSystemHandler(ISystemInstallation systemInstallation)
    : ICommandHandler<InstallSystemCommand, Result<InstallSystemResponse>>
{
    public async ValueTask<Result<InstallSystemResponse>> Handle(InstallSystemCommand command, CancellationToken ct)
    {
        // 1. Guard: Check if already installed (Directly via service, NO Mediator)
        var status = await systemInstallation.GetInstallationStatus(ct);

        if (status.IsSuccessful && status.Data?.IsInitialized is true)
        {
            return Result<InstallSystemResponse>.Failure(
                ResultError.Conflict("Installation", "System is already initialized."));
        }

        // 2. Guard: Test Connection
        var dbDto = new DbConnectionDto(
            command.DatabaseHost, command.DatabasePort,
            command.DatabaseName, command.DatabaseUsername, command.DatabasePassword);

        if (!await systemInstallation.TestDatabaseConnection(dbDto, ct))
        {
            return Result<InstallSystemResponse>.Failure(
                ResultError.BadRequest("Installation", "Database connection failed. Verify your credentials."));
        }

        // 3. Execute: Perform the installation
        var result = await systemInstallation.InstallSystem(command, ct);

        return result.Data is not null ? Result<InstallSystemResponse>.Success(new InstallSystemResponse()
        {
            TenantId = result.Data.TenantId,
            TenantIdentifier = result.Data.TenantIdentifier,
            SuperUserId = result.Data.SuperUserId,
            SuperUserEmail = result.Data.SuperUserEmail,
            SchemaVersion = result.Data.SchemaVersion,
            EnvironmentVariable = result.Data.EnvironmentVariable
        }).WithMessage("System installation completed successfully. Please update your Docker environment to persist these settings.")
        :
                Result<InstallSystemResponse>.Failure(ResultError.InternalError("Installation", "Unknown error occurred during installation."));
        ;
    }
}
