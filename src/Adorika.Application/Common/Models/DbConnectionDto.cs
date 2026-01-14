namespace Adorika.Application.Common.Models;

public sealed record DbConnectionDto(string Host, int Port, string Database, string User, string Pass);
