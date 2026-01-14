using Adorika.Api.Common.Response;
using Adorika.Application.Features.Installation.GetInstallationStatus;
using Adorika.Application.Features.Installation.InstallSystem;
using Adorika.Application.Features.Installation.TestDatabaseConnection;

using Mediator;

using Microsoft.AspNetCore.Mvc;

namespace Adorika.Api.Endpoints;

public static class InstallationEndpoints
{
    public static IEndpointRouteBuilder MapInstallationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/install")
            .WithTags("Installation");

        // 1. Status Check
        group.MapGet("/status", async (IMediator mediator, CancellationToken ct) =>
                await mediator.Send(new InstallationStatusQuery(), ct))
            .WithName("GetInstallationStatus")
            .WithLinks<InstallationStatusResponse>(data => [
                data.IsInitialized
                    ? new LinkSpec("Login", "login", "POST", Title: "Proceed to Login")
                    : new LinkSpec("InstallSystem", "install", "POST", Title: "Start Installation")
            ]);

        // 2. Test Connection
        group.MapPost("/test-connection", async ([FromBody] DatabaseConnectionCommand cmd, IMediator mediator, CancellationToken ct) =>
                await mediator.Send(cmd, ct))
            .WithName("TestDatabaseConnection")
            .WithLinks<DatabaseConnectionResponse>(_ => [
                new LinkSpec("InstallSystem", "install", "POST")
            ]);

        // 3. Perform Installation
        group.MapPost("", async ([FromBody] InstallSystemCommand cmd, IMediator mediator, CancellationToken ct) => await mediator.Send(cmd, ct))
        .WithName("InstallSystem")
        .WithLinks<InstallSystemResponse>(_ => [
            new LinkSpec("GetInstallationStatus", "status", "GET"),
            new LinkSpec("Login", "login", "POST")
        ]);

        return app;
    }
}

