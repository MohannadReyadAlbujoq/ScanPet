using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Queries.SearchOrders;

/// <summary>
/// Query to search orders by order number or client name
/// Inherits pagination and search term from BaseSearchQuery
/// </summary>
public class SearchOrdersQuery : BaseSearchQuery<OrderDto>, IRequest<Result<List<OrderDto>>>
{
}
