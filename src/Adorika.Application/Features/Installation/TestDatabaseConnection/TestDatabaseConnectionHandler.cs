using Adorika.Application.Common.Interfaces.Services;
using Adorika.Application.Common.Models;
using Adorika.Application.Common.Wrapper;

using Mediator;

namespace Adorika.Application.Features.Installation.TestDatabaseConnection;

/// <summary>
/// Handler for testing database connection before installation.
/// Returns standardized Result with connection status and helpful links.
/// </summary>
public sealed class TestDatabaseConnectionHandler(ISystemInstallation systemInstallation)
    : IQueryHandler<TestDatabaseConnectionQuery, Result<DatabaseConnectionResponse>>
{
    public async ValueTask<Result<DatabaseConnectionResponse>> Handle(TestDatabaseConnectionQuery query, CancellationToken cancellationToken)
    {

        // 1. Build database connection DTO
        var dbDto = new DbConnectionDto(
            query.Host, query.Port,
            query.Database, query.Username, query.Password);

        // 2. Call the infrastructure service to test connection
        var isConnected = await systemInstallation.TestDatabaseConnection(dbDto, cancellationToken);

        // 3. Return success result with connection status
        if (isConnected)
        {
            return Result<DatabaseConnectionResponse>.Success()
                .WithData(new DatabaseConnectionResponse(true))
                .WithMessage("Database connection test successful. You can proceed with installation.");
        }

        // 4. Connection failed - provide helpful error with troubleshooting links
        return Result<DatabaseConnectionResponse>.Failure(ResultError.BadRequest("DatabaseConnection",
                "Failed to connect to database. Please verify the following:\n" +
                "- Database server is running and accessible\n" +
                "- Host and port are correct\n" +
                "- Database name exists or user has permission to create it\n" +
                "- Username and password are correct\n" +
                "- Firewall allows connections on the specified port"));
    }
}
