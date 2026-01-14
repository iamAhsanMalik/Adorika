using Adorika.Application.Common.Wrapper;

using Mediator;

namespace Adorika.Application.Features.Installation.TestDatabaseConnection;

/// <summary>
/// Query to test database connection before installation.
/// This is a query, not a command, as it does not mutate state.
/// </summary>
public record TestDatabaseConnectionQuery(
    string Host,
    int Port,
    string Database,
    string Username,
    string Password) : IQuery<Result<DatabaseConnectionResponse>>;
