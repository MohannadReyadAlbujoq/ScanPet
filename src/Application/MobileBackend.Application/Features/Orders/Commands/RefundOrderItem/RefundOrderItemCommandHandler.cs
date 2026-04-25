using MediatR;
using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Orders;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;

/// <summary>
/// v5 handler: refunds one or more lines of an order in a single call.
/// - Validates order, inventory and per-line quantities
/// - Increments inventory for each refunded line
/// - Updates per-line refund tracking (RefundedQuantity, Status, reason)
/// - Promotes Order.OrderStatus to Refunded (all lines fully refunded) or PartiallyRefunded
/// </summary>
public class RefundOrderItemCommandHandler : IRequestHandler<RefundOrderItemCommand, Result<RefundResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RefundOrderItemCommandHandler> _logger;

    public RefundOrderItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<RefundOrderItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<RefundResultDto>> Handle(RefundOrderItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.OrderId == Guid.Empty)
                return Result<RefundResultDto>.FailureResult("OrderId is required", 400);

            if (request.Items == null || request.Items.Count == 0)
                return Result<RefundResultDto>.FailureResult("At least one item must be provided", 400);

            var order = await _unitOfWork.Orders.GetWithItemsAsync(request.OrderId, cancellationToken);
            if (order == null)
                return Result<RefundResultDto>.FailureResult($"Order {request.OrderId} not found", 404);

            if (order.IsDeleted)
                return Result<RefundResultDto>.FailureResult("Order has been deleted", 400);

            if (order.OrderStatus == OrderStatus.Cancelled)
                return Result<RefundResultDto>.FailureResult("Cannot refund a cancelled order", 400);

            if (order.OrderStatus == OrderStatus.Refunded)
                return Result<RefundResultDto>.FailureResult("Order is already fully refunded", 400);

            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.RefundToInventoryId, cancellationToken);
            if (inventory == null)
                return Result<RefundResultDto>.FailureResult($"Inventory {request.RefundToInventoryId} not found", 404);

            if (!inventory.IsActive)
                return Result<RefundResultDto>.FailureResult($"Cannot refund to inactive inventory: {inventory.Name}", 400);

            var summaries = new List<RefundedLineSummaryDto>();

            foreach (var line in request.Items)
            {
                if (line.Quantity <= 0)
                    return Result<RefundResultDto>.FailureResult("Refund quantity must be greater than 0", 400);

                if (line.OrderItemId == null && line.ItemId == null)
                    return Result<RefundResultDto>.FailureResult("Each refund line must include OrderItemId or ItemId", 400);

                var orderItem = order.OrderItems.FirstOrDefault(oi =>
                    !oi.IsDeleted &&
                    ((line.OrderItemId.HasValue && oi.Id == line.OrderItemId.Value) ||
                     (!line.OrderItemId.HasValue && line.ItemId.HasValue && oi.ItemId == line.ItemId.Value)));

                if (orderItem == null)
                    return Result<RefundResultDto>.FailureResult(
                        $"Order line not found (OrderItemId={line.OrderItemId}, ItemId={line.ItemId})", 404);

                if (orderItem.Status == OrderItemStatus.Refunded)
                    return Result<RefundResultDto>.FailureResult(
                        $"Order line {orderItem.Id} is already fully refunded", 400);

                var remaining = orderItem.Quantity - orderItem.RefundedQuantity;
                if (line.Quantity > remaining)
                    return Result<RefundResultDto>.FailureResult(
                        $"Refund quantity ({line.Quantity}) exceeds remaining ({remaining}) for line {orderItem.Id}", 400);

                var adjusted = await _unitOfWork.ItemInventories.AdjustInventoryAsync(
                    itemId: orderItem.ItemId,
                    inventoryId: request.RefundToInventoryId,
                    quantityChange: line.Quantity,
                    cancellationToken: cancellationToken);

                if (!adjusted)
                    return Result<RefundResultDto>.FailureResult(
                        $"Failed to restore inventory for item {orderItem.ItemId}", 500);

                var newRefunded = orderItem.RefundedQuantity + line.Quantity;
                orderItem.RefundedQuantity = newRefunded;
                orderItem.RefundedAt = DateTime.UtcNow;
                orderItem.RefundedBy = _currentUserService.UserId;
                orderItem.RefundedToInventoryId = request.RefundToInventoryId;
                orderItem.RefundReason = string.IsNullOrEmpty(orderItem.RefundReason)
                    ? request.RefundReason
                    : (string.IsNullOrEmpty(request.RefundReason) ? orderItem.RefundReason : $"{orderItem.RefundReason}; {request.RefundReason}");
                orderItem.UpdatedAt = DateTime.UtcNow;
                orderItem.UpdatedBy = _currentUserService.UserId;
                orderItem.Status = newRefunded >= orderItem.Quantity
                    ? OrderItemStatus.Refunded
                    : OrderItemStatus.PartiallyRefunded;

                _unitOfWork.OrderItems.Update(orderItem);

                summaries.Add(new RefundedLineSummaryDto
                {
                    OrderItemId = orderItem.Id,
                    ItemId = orderItem.ItemId,
                    ItemName = orderItem.Item?.Name,
                    ItemImageUrl = orderItem.Item?.ImageUrl,
                    SerialNumber = orderItem.SerialNumber,
                    Quantity = line.Quantity,
                    TotalRefundedQuantity = newRefunded,
                    OrderedQuantity = orderItem.Quantity,
                    RefundedAmount = orderItem.SalePrice * newRefunded,
                    RefundedPercent = orderItem.Quantity > 0
                        ? Math.Round((decimal)newRefunded / orderItem.Quantity * 100m, 2)
                        : 0m,
                    Status = (int)orderItem.Status,
                    StatusName = orderItem.Status.ToString()
                });

                await _auditService.LogAsync(
                    action: AuditActions.OrderItemRefunded,
                    entityName: EntityNames.OrderItem,
                    entityId: orderItem.Id,
                    userId: _currentUserService.UserId ?? Guid.Empty,
                    additionalInfo: $"Refunded {line.Quantity} of OrderItem {orderItem.Id} (Total {newRefunded}/{orderItem.Quantity}), Status: {orderItem.Status}, Inventory: {inventory.Name}, Reason: {request.RefundReason ?? "n/a"}",
                    cancellationToken: cancellationToken);
            }

            // Promote order status
            var allItems = order.OrderItems.Where(oi => !oi.IsDeleted).ToList();
            var totalOrderedQty = allItems.Sum(oi => oi.Quantity);
            var totalRefundedQty = allItems.Sum(oi => oi.RefundedQuantity);
            var totalRefundedAmount = allItems.Sum(oi => oi.SalePrice * oi.RefundedQuantity);

            if (totalRefundedQty > 0 && totalRefundedQty >= totalOrderedQty)
                order.OrderStatus = OrderStatus.Refunded;
            else if (totalRefundedQty > 0)
                order.OrderStatus = OrderStatus.PartiallyRefunded;

            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _currentUserService.UserId;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new RefundResultDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = (int)order.OrderStatus,
                OrderStatusName = order.OrderStatus.ToString(),
                RefundedPercent = totalOrderedQty > 0
                    ? Math.Round((decimal)totalRefundedQty / totalOrderedQty * 100m, 2)
                    : 0m,
                RefundedAmount = totalRefundedAmount,
                RefundedItems = summaries
            };

            _logger.LogInformation(
                "Order {OrderId} refund processed. Status={Status}, Refunded={Refunded}/{Total} ({Percent}%), Amount={Amount}",
                order.Id, order.OrderStatus, totalRefundedQty, totalOrderedQty, result.RefundedPercent, result.RefundedAmount);

            return Result<RefundResultDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding order {OrderId}", request.OrderId);
            return Result<RefundResultDto>.FailureResult("An error occurred while processing the refund", 500);
        }
    }
}
