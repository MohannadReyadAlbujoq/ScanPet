using FluentValidation;
using MobileBackend.Application.Features.Roles.Commands.CreateRole;

namespace MobileBackend.Application.Validators.Roles;

/// <summary>
/// Validator for CreateRoleCommand
/// Validates required fields for role creation
/// </summary>
public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Role name must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Role name can only contain letters, numbers, underscores, dots and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
