using FluentValidation;
using MobileBackend.Application.Features.Items.Commands.UpdateItem;

namespace MobileBackend.Application.Validators.Items;

/// <summary>
/// Validator for UpdateItemCommand
/// </summary>
public class UpdateItemValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required")
            .MinimumLength(2).WithMessage("Item name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Item name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.SKU)
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.SKU));

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price must be greater than or equal to 0");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity must be greater than or equal to 0");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}
