using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Command to cancel an order
/// </summary>
public class CancelOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
    public string? CancellationReason { get; set; }
}
