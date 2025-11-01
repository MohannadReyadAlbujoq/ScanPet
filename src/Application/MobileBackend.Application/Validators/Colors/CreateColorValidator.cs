using FluentValidation;
using MobileBackend.Application.Features.Colors.Commands.CreateColor;

namespace MobileBackend.Application.Validators.Colors;

/// <summary>
/// Validator for CreateColorCommand
/// </summary>
public class CreateColorValidator : AbstractValidator<CreateColorCommand>
{
    public CreateColorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Color name is required")
            .MinimumLength(2).WithMessage("Color name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Color name must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.RedValue)
            .InclusiveBetween(0, 255).WithMessage("Red value must be between 0 and 255");

        RuleFor(x => x.GreenValue)
            .InclusiveBetween(0, 255).WithMessage("Green value must be between 0 and 255");

        RuleFor(x => x.BlueValue)
            .InclusiveBetween(0, 255).WithMessage("Blue value must be between 0 and 255");
    }
}
