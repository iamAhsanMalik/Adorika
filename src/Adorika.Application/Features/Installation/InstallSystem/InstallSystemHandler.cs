using Adorika.Application.Common.Interfaces.Services;
using Adorika.Application.Common.Wrapper;
using Adorika.Application.Features.Installation.GetInstallationStatus;
using Adorika.Application.Features.Installation.InstallSystem;
using Adorika.Application.Features.Installation.TestDatabaseConnection;

using Mediator;

using Microsoft.AspNetCore.Http;

public sealed class InstallSystemHandler(
    ISystemInstallation systemInstallation,
    IMediator mediator,
    IHttpContextAccessor httpContextAccessor)
    : ICommandHandler<InstallSystemCommand, Result<InstallSystemResponse>>
{
    public async ValueTask<Result<InstallSystemResponse>> Handle(InstallSystemCommand command, CancellationToken ct)
    {
        // 1. Guard: Check if already installed (Using Mediator for CQRS compliance)
        var status = await mediator.Send(new InstallationStatusQuery(), ct);

        if (status.IsSuccessful && status.Data?.IsInitialized is true)
        {
            return Result<InstallSystemResponse>.Failure(
                ResultError.Conflict("Installation", "System is already initialized."));
        }

        // 2. Guard: Test Connection (Using Mediator for CQRS compliance)
        var connectionTest = await mediator.Send(new TestDatabaseConnectionQuery(
            command.DatabaseHost,
            command.DatabasePort,
            command.DatabaseName,
            command.DatabaseUsername,
            command.DatabasePassword), ct);

        if (!connectionTest.IsSuccessful || connectionTest.Data?.IsConnected != true)
        {
            return Result<InstallSystemResponse>.Failure(
                ResultError.BadRequest("Installation", "Database connection failed. Verify your credentials."));
        }

        // 3. Get client context for audit trail
        var httpContext = httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

        // 4. Execute: Perform the installation
        var result = await systemInstallation.InstallSystem(command, ipAddress, userAgent, ct);

        if (result.Data is not null)
        {
            return Result<InstallSystemResponse>.Success(new InstallSystemResponse
            {
                TenantId = result.Data.TenantId,
                TenantIdentifier = result.Data.TenantIdentifier,
                SuperUserId = result.Data.SuperUserId,
                SuperUserEmail = result.Data.SuperUserEmail,
                SchemaVersion = result.Data.SchemaVersion,
                // Don't expose connection string - just indicate configuration is needed
                DatabaseConfigured = true
            }).WithMessage("System installation completed successfully. Please configure your connection string in environment variables.");
        }

        return Result<InstallSystemResponse>.Failure(
            ResultError.InternalError("Installation", "Unknown error occurred during installation."));
    }
}
