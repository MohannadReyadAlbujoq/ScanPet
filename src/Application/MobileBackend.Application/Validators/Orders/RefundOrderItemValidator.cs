using FluentValidation;

namespace MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;

/// <summary>
/// Validator for v5 order-level RefundOrderItemCommand
/// </summary>
public class RefundOrderItemValidator : AbstractValidator<RefundOrderItemCommand>
{
    public RefundOrderItemValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.RefundToInventoryId)
            .NotEmpty().WithMessage("RefundToInventoryId is required");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items array is required")
            .Must(items => items != null && items.Count > 0).WithMessage("At least one item must be provided");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Refund quantity must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Refund quantity seems unreasonably high");

            item.RuleFor(i => i)
                .Must(i => i.OrderItemId.HasValue || i.ItemId.HasValue)
                .WithMessage("Each refund line must include OrderItemId or ItemId");
        });

        RuleFor(x => x.RefundReason)
            .MaximumLength(500).WithMessage("Refund reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.RefundReason));
    }
}
