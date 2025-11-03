using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Query to get all orders with optional pagination
/// </summary>
public class GetAllOrdersQuery : BasePagedQuery<OrderDto>
{
}
