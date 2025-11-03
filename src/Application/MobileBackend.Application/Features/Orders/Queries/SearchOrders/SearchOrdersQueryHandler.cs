using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Orders.Queries.SearchOrders;

/// <summary>
/// Handler for searching orders by order number or client name
/// Uses BaseSearchHandler to eliminate code duplication
/// </summary>
public class SearchOrdersQueryHandler : BaseSearchHandler<SearchOrdersQuery, Order, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public SearchOrdersQueryHandler(
        IOrderRepository orderRepository,
        ILogger<SearchOrdersQueryHandler> logger)
        : base(logger)
    {
        _orderRepository = orderRepository;
    }

    protected override async Task<List<Order>> GetAllEntitiesAsync(CancellationToken cancellationToken)
    {
        // Use eager loading for better performance
        var orders = await _orderRepository.GetAllWithLocationsAsync(cancellationToken);
        return orders.ToList();
    }

    protected override bool MatchesSearchTerm(Order entity, string searchTerm)
    {
        return entity.OrderNumber.ToLower().Contains(searchTerm) ||
               entity.ClientName.ToLower().Contains(searchTerm) ||
               (entity.ClientPhone != null && entity.ClientPhone.Contains(searchTerm)) ||
               (entity.Description != null && entity.Description.ToLower().Contains(searchTerm));
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
            LocationName = entity.Location?.Name,
            Description = entity.Description,
            TotalAmount = entity.TotalAmount,
            OrderStatus = (int)entity.OrderStatus,
            OrderStatusName = entity.OrderStatus.ToString(),
            OrderDate = entity.OrderDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Order;
}
