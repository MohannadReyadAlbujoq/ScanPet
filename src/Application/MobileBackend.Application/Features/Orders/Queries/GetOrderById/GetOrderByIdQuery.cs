using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Query to get an order by ID with items
/// </summary>
public class GetOrderByIdQuery : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; set; }
}
