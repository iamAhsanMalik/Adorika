using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;
using Adorika.Infrastructure.Services;

using Mediator;

namespace Adorika.Application.Features.Installation.TestDatabaseConnection;

/// <summary>
/// Handler for testing database connection before installation.
/// Returns standardized Result with connection status and helpful links.
/// </summary>
public sealed class DatabaseConnectionHandler(ISystemInstallation systemInstallation)
    : ICommandHandler<DatabaseConnectionCommand, Result<DatabaseConnectionResponse>>
{
    private readonly ISystemInstallation _systemInstallation = systemInstallation;

    public async ValueTask<Result<DatabaseConnectionResponse>> Handle(DatabaseConnectionCommand command, CancellationToken cancellationToken)
    {

        // 2. Guard: Test Connection
        var dbDto = new DbConnectionDto(
            command.Host, command.Port,
            command.Database, command.Username, command.Password);

        // 1. Call the infrastructure service to test connection
        var isConnected = await _systemInstallation.TestDatabaseConnection(dbDto, cancellationToken);

        // 2. Return success result with connection status
        if (isConnected)
        {
            return Result<DatabaseConnectionResponse>.Success()
                .WithData(new DatabaseConnectionResponse(true))
                .WithMessage("Database connection test successful. You can proceed with installation.");
        }

        // 3. Connection failed - provide helpful error with troubleshooting links
        return Result<DatabaseConnectionResponse>.Failure(ResultError.BadRequest("DatabaseConnection",
                "Failed to connect to database. Please verify the following:\n" +
                "- Database server is running and accessible\n" +
                "- Host and port are correct\n" +
                "- Database name exists or user has permission to create it\n" +
                "- Username and password are correct\n" +
                "- Firewall allows connections on the specified port"));
    }
}
