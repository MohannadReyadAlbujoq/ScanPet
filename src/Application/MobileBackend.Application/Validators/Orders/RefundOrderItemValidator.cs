using FluentValidation;

namespace MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;

/// <summary>
/// Validator for RefundOrderItemCommand
/// </summary>
public class RefundOrderItemValidator : AbstractValidator<RefundOrderItemCommand>
{
    public RefundOrderItemValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Serial number is required")
            .MaximumLength(100).WithMessage("Serial number cannot exceed 100 characters");

        RuleFor(x => x.RefundQuantity)
            .GreaterThan(0).WithMessage("Refund quantity must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Refund quantity seems unreasonably high");

        RuleFor(x => x.RefundReason)
            .MaximumLength(500).WithMessage("Refund reason cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.RefundReason));
    }
}
