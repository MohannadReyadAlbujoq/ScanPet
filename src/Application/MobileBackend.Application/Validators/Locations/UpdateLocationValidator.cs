using FluentValidation;
using MobileBackend.Application.Features.Locations.Commands.UpdateLocation;

namespace MobileBackend.Application.Validators.Locations;

/// <summary>
/// Validator for UpdateLocationCommand
/// </summary>
public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
{
    public UpdateLocationValidator()
    {
        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Location name is required")
            .MinimumLength(2).WithMessage("Location name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Location name must not exceed 100 characters");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));
    }
}
