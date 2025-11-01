using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;

/// <summary>
/// Command to refund an order item by serial number
/// </summary>
public class RefundOrderItemCommand : IRequest<Result>
{
    public string SerialNumber { get; set; } = string.Empty;
    public int RefundQuantity { get; set; }
    public string? RefundReason { get; set; }
}
