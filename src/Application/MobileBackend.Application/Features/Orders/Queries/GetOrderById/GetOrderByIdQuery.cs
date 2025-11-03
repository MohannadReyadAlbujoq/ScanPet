using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;

namespace MobileBackend.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Query to get an order by ID with items
/// </summary>
public class GetOrderByIdQuery : BaseGetByIdQuery<OrderDto>
{
    // Backwards compatibility: Allow OrderId property
    public Guid OrderId 
    { 
        get => Id; 
        set => Id = value; 
    }
}
