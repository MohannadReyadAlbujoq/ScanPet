using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Query to get all orders with optional pagination and status filter
/// </summary>
public class GetAllOrdersQuery : BasePagedQuery<OrderDto>
{
    /// <summary>
    /// Optional status filter (0=Pending, 1=Confirmed, 2=Cancelled)
    /// </summary>
    public int? Status { get; set; }
}
