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
        var items = entity.OrderItems?.Where(oi => !oi.IsDeleted).ToList() ?? new List<OrderItem>();
        var totalOrderedQty = items.Sum(oi => oi.Quantity);
        var totalRefundedQty = items.Sum(oi => oi.RefundedQuantity);
        var totalRefundedAmount = items.Sum(oi => oi.SalePrice * oi.RefundedQuantity);

        return new OrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            ClientName = entity.ClientName,
            ClientPhone = entity.ClientPhone,
            InventoryId = entity.InventoryId,
            InventoryName = entity.Inventory?.Name,
            Description = entity.Description,
            OrderDate = entity.OrderDate,
            TotalAmount = entity.TotalAmount,
            OrderStatus = (int)entity.OrderStatus,
            OrderStatusName = entity.OrderStatus.ToString(),
            OrderItems = items.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ItemId = oi.ItemId,
                ItemName = oi.Item?.Name,
                ItemImageUrl = oi.Item?.ImageUrl,
                SerialNumber = oi.SerialNumber,
                Quantity = oi.Quantity,
                UnitPrice = oi.SalePrice,
                TotalPrice = oi.SalePrice * oi.Quantity,
                Status = (int)oi.Status,
                StatusName = oi.Status.ToString(),
                RefundedQuantity = oi.RefundedQuantity,
                RefundedAmount = oi.RefundedQuantity > 0 ? oi.SalePrice * oi.RefundedQuantity : null,
                RefundedPercent = oi.RefundedQuantity > 0 && oi.Quantity > 0
                    ? Math.Round((decimal)oi.RefundedQuantity / oi.Quantity * 100m, 2)
                    : null,
                RefundReason = oi.RefundReason,
                RefundedAt = oi.RefundedAt
            }).ToList(),
            RefundedQuantity = totalRefundedQty > 0 ? totalRefundedQty : null,
            RefundedAmount = totalRefundedQty > 0 ? totalRefundedAmount : null,
            RefundedPercent = totalRefundedQty > 0 && totalOrderedQty > 0
                ? Math.Round((decimal)totalRefundedQty / totalOrderedQty * 100m, 2)
                : null,
            RefundedItems = items.Where(oi => oi.RefundedQuantity > 0).Select(oi => new RefundedItemSummary
            {
                OrderItemId = oi.Id,
                ItemId = oi.ItemId,
                RefundedQuantity = oi.RefundedQuantity,
                OrderedQuantity = oi.Quantity,
                RefundedAmount = oi.SalePrice * oi.RefundedQuantity,
                RefundedPercent = oi.Quantity > 0
                    ? Math.Round((decimal)oi.RefundedQuantity / oi.Quantity * 100m, 2)
                    : 0m
            }).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Order;
}
