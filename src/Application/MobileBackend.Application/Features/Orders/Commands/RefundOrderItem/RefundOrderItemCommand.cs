using MediatR;
using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;

/// <summary>
/// Command to refund one or more lines of an order.
/// v5: order-level refund with explicit items[] payload.
/// </summary>
public class RefundOrderItemCommand : IRequest<Result<RefundResultDto>>
{
    public Guid OrderId { get; set; }
    public Guid RefundToInventoryId { get; set; }
    public string? RefundReason { get; set; }
    public List<RefundLineDto> Items { get; set; } = new();
}
