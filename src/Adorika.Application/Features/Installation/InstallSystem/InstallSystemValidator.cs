using FluentValidation;

namespace Adorika.Application.Features.Installation.InstallSystem;

public class InstallSystemValidator : AbstractValidator<InstallSystemCommand>
{
    public InstallSystemValidator()
    {
        // Tenant Validation
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters");

        RuleFor(x => x.TenantIdentifier)
            .NotEmpty().WithMessage("Tenant identifier is required")
            .MinimumLength(3).WithMessage("Tenant identifier must be at least 3 characters")
            .MaximumLength(50).WithMessage("Tenant identifier must not exceed 50 characters")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Tenant identifier must contain only lowercase letters, numbers, and hyphens")
            .Must(x => !x.StartsWith("-") && !x.EndsWith("-")).WithMessage("Tenant identifier cannot start or end with a hyphen")
            .Must(x => !x.Contains("--")).WithMessage("Tenant identifier cannot contain consecutive hyphens");

        RuleFor(x => x.TenantContactEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.TenantContactEmail))
            .WithMessage("A valid tenant contact email is required");

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
            .MinimumLength(8).WithMessage("Super user password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Super user password must not exceed 128 characters")
            .Matches(@"[A-Z]").WithMessage("Super user password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Super user password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Super user password must contain at least one number")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Super user password must contain at least one special character");

        // Database Validation
        RuleFor(x => x.DatabaseHost)
            .NotEmpty().WithMessage("Database host is required")
            .MaximumLength(255).WithMessage("Database host must not exceed 255 characters");

        RuleFor(x => x.DatabasePort)
            .InclusiveBetween(1, 65535).WithMessage("Database port must be between 1 and 65535");

        RuleFor(x => x.DatabaseName)
            .NotEmpty().WithMessage("Database name is required")
            .MaximumLength(63).WithMessage("Database name must not exceed 63 characters")
            .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$")
            .WithMessage("Database name must start with a letter or underscore and contain only letters, numbers, and underscores");

        RuleFor(x => x.DatabaseUsername)
            .NotEmpty().WithMessage("Database username is required")
            .MaximumLength(63).WithMessage("Database username must not exceed 63 characters");

        RuleFor(x => x.DatabasePassword)
            .NotEmpty().WithMessage("Database password is required");
    }
}
