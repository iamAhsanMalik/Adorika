using Adorika.Application.Common.Wrapper;

using Mediator;

namespace Adorika.Application.Features.Installation.TestDatabaseConnection;

public record DatabaseConnectionCommand(
    string Host,
    int Port,
    string Database,
    string Username,
    string Password) : ICommand<Result<DatabaseConnectionResponse>>;
