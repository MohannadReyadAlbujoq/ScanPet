using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Command to create a new order with items
/// </summary>
public class CreateOrderCommand : IRequest<Result<Guid>>
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string? Description { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
