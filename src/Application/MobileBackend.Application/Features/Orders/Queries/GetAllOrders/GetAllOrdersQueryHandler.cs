using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Orders.Queries.GetAllOrders;

/// <summary>
/// Handler for getting all orders (with locations included - no N+1)
/// Uses BaseGetAllHandler to eliminate code duplication
/// </summary>
public class GetAllOrdersQueryHandler : BaseGetAllHandler<GetAllOrdersQuery, Order, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllOrdersQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<List<Order>> GetEntitiesAsync(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        // Use optimized method that includes locations ?
        var orders = await _unitOfWork.Orders.GetAllWithLocationsAsync(cancellationToken);
        return orders.ToList();
    }

    protected override OrderDto MapToDto(Order entity)
    {
        return new OrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            ClientName = entity.ClientName,
            ClientPhone = entity.ClientPhone,
            LocationId = entity.LocationId,
            LocationName = entity.Location?.Name,  // ? Now works correctly!
            Description = entity.Description,
            OrderDate = entity.OrderDate,
            TotalAmount = entity.TotalAmount,
            OrderStatus = (int)entity.OrderStatus,
            OrderStatusName = entity.OrderStatus.ToString(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Order;
}
