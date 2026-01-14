using FluentValidation;

namespace Adorika.Application.Features.Installation.InstallSystem;

public class InstallSystemValidator : AbstractValidator<InstallSystemCommand>
{
    public InstallSystemValidator()
    {
        // Tenant Validation
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required");

        RuleFor(x => x.TenantIdentifier)
            .NotEmpty().WithMessage("Tenant identifier is required");

        // Super User Validation
        RuleFor(x => x.SuperUserFirstName)
            .NotEmpty().WithMessage("Super user first name is required");

        RuleFor(x => x.SuperUserLastName)
            .NotEmpty().WithMessage("Super user last name is required");

        RuleFor(x => x.SuperUserEmail)
            .NotEmpty().WithMessage("Super user email is required")
            .EmailAddress().WithMessage("A valid email is required");

        RuleFor(x => x.SuperUserPassword)
            .NotEmpty().WithMessage("Super user password is required")
            .MinimumLength(8).WithMessage("Super user password must be at least 8 characters");

        // Database Validation
        RuleFor(x => x.DatabaseHost)
            .NotEmpty().WithMessage("Database host is required");

        RuleFor(x => x.DatabasePort)
            .InclusiveBetween(1, 65535).WithMessage("Database port must be between 1 and 65535");

        RuleFor(x => x.DatabaseName)
            .NotEmpty().WithMessage("Database name is required");

        RuleFor(x => x.DatabaseUsername)
            .NotEmpty().WithMessage("Database username is required");

        RuleFor(x => x.DatabasePassword)
            .NotEmpty().WithMessage("Database password is required");
    }
}
