using FluentValidation;

namespace Adorika.Application.Features.Installation.TestDatabaseConnection;

/// <summary>
/// Validator for database connection test requests.
/// Ensures all required connection parameters are valid before attempting connection.
/// </summary>
public class DatabaseConnectionValidator : AbstractValidator<DatabaseConnectionCommand>
{
    public DatabaseConnectionValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Database host is required")
            .MaximumLength(255).WithMessage("Database host must not exceed 255 characters");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Database port must be between 1 and 65535");

        RuleFor(x => x.Database)
            .NotEmpty().WithMessage("Database name is required")
            .MaximumLength(63).WithMessage("Database name must not exceed 63 characters")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$")
            .WithMessage("Database name must start with a letter or underscore and contain only letters, numbers, and underscores");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Database username is required")
            .MaximumLength(63).WithMessage("Database username must not exceed 63 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Database password is required");
    }
}
