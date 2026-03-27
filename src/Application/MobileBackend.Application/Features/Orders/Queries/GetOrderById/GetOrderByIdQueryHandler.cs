using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Handler for getting an order by ID with items
/// </summary>
public class GetOrderByIdQueryHandler : BaseGetByIdHandler<GetOrderByIdQuery, Order, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrderByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetOrderByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Order?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Orders.GetWithItemsAsync(id, cancellationToken);
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
            OrderDate = entity.OrderDate,
            TotalAmount = entity.TotalAmount,
            OrderStatus = (int)entity.OrderStatus,
            OrderStatusName = entity.OrderStatus.ToString(),
            OrderItems = entity.OrderItems?.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ItemId = oi.ItemId,
                ItemName = oi.Item?.Name,
                SerialNumber = oi.SerialNumber,
                Quantity = oi.Quantity,
                UnitPrice = oi.SalePrice,
                TotalPrice = oi.SalePrice * oi.Quantity,
                Status = (int)oi.Status,
                StatusName = oi.Status.ToString()
            }).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Order;
}
