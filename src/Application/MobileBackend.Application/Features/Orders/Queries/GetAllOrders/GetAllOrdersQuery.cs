using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Query to get all orders with optional pagination
/// </summary>
public class GetAllOrdersQuery : IRequest<Result<List<OrderDto>>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
}
