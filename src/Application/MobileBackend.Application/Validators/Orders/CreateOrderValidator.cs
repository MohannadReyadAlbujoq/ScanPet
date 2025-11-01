using FluentValidation;
using MobileBackend.Application.Features.Orders.Commands.CreateOrder;

namespace MobileBackend.Application.Validators.Orders;

/// <summary>
/// Validator for CreateOrderCommand
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty().WithMessage("Client name is required")
            .MaximumLength(100).WithMessage("Client name must not exceed 100 characters");

        RuleFor(x => x.ClientPhone)
            .NotEmpty().WithMessage("Client phone is required")
            .MaximumLength(20).WithMessage("Client phone must not exceed 20 characters");

        RuleFor(x => x.LocationId)
            .NotEmpty().WithMessage("Location ID is required");

        RuleFor(x => x.OrderItems)
            .NotNull().WithMessage("Order must contain items")
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(x => x.OrderItems).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemId)
                .NotEmpty().WithMessage("Item ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0")
                .When(x => x.UnitPrice.HasValue);
        });

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
