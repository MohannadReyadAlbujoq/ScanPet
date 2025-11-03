using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Orders.Commands.RefundOrderItem;

/// <summary>
/// Handler for refunding an order item by serial number
/// NEW: Uses Inventory system for proper warehouse tracking
/// </summary>
public class RefundOrderItemCommandHandler : IRequestHandler<RefundOrderItemCommand, Result>
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

    public async Task<Result> Handle(RefundOrderItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate refund quantity first
            if (request.RefundQuantity <= 0)
            {
                return Result.FailureResult("Refund quantity must be greater than 0", 400);
            }

            // Find order item by serial number
            var orderItem = await _unitOfWork.OrderItems.GetBySerialNumberAsync(request.SerialNumber, cancellationToken);
            if (orderItem == null)
            {
                return Result.FailureResult($"Order item with serial number {request.SerialNumber} not found", 404);
            }

            // Check if order item is already refunded
            if (orderItem.Status == OrderItemStatus.Refunded)
            {
                return Result.FailureResult("This order item has already been refunded", 400);
            }

            // Check if order item is deleted
            if (orderItem.IsDeleted)
            {
                return Result.FailureResult("This order item has been deleted", 400);
            }

            // Validate refund quantity
            if (request.RefundQuantity > orderItem.Quantity)
            {
                return Result.FailureResult(
                    $"Refund quantity ({request.RefundQuantity}) cannot exceed ordered quantity ({orderItem.Quantity})", 
                    400);
            }

            // Validate inventory exists
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.RefundToInventoryId, cancellationToken);
            if (inventory == null)
            {
                return Result.FailureResult(
                    $"Inventory with ID {request.RefundToInventoryId} not found", 
                    404);
            }

            // Check if inventory is active
            if (!inventory.IsActive)
            {
                return Result.FailureResult(
                    $"Cannot refund to inactive inventory: {inventory.Name}", 
                    400);
            }

            // Get the related item to verify it exists
            var item = await _unitOfWork.Items.GetByIdAsync(orderItem.ItemId, cancellationToken);
            if (item == null)
            {
                return Result.FailureResult("Related item not found in inventory", 404);
            }

            // NEW: Restore inventory to specific warehouse using ItemInventory
            var adjustSuccess = await _unitOfWork.ItemInventories.AdjustInventoryAsync(
                itemId: orderItem.ItemId,
                inventoryId: request.RefundToInventoryId,
                quantityChange: request.RefundQuantity, // Positive = add back to inventory
                cancellationToken: cancellationToken
            );

            if (!adjustSuccess)
            {
                return Result.FailureResult(
                    "Failed to restore inventory. Please contact support.", 
                    500);
            }

            // Update order item status to refunded
            orderItem.Status = OrderItemStatus.Refunded;
            orderItem.RefundedQuantity = request.RefundQuantity;
            orderItem.RefundedAt = DateTime.UtcNow;
            orderItem.RefundedBy = _currentUserService.UserId;
            orderItem.RefundReason = request.RefundReason;
            orderItem.RefundedToInventoryId = request.RefundToInventoryId; // Track where it went
            orderItem.UpdatedAt = DateTime.UtcNow;
            orderItem.UpdatedBy = _currentUserService.UserId;

            // Update order item
            _unitOfWork.OrderItems.Update(orderItem);

            // Save all changes in transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log for order item refund
            await _auditService.LogAsync(
                action: AuditActions.OrderItemRefunded,
                entityName: EntityNames.OrderItem,
                entityId: orderItem.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Refunded order item {request.SerialNumber}, " +
                               $"Quantity: {request.RefundQuantity}, " +
                               $"To Inventory: {inventory.Name} ({inventory.Id}), " +
                               $"Reason: {request.RefundReason ?? "Not specified"}",
                cancellationToken: cancellationToken
            );

            // Audit log for inventory restoration
            await _auditService.LogAsync(
                action: AuditActions.ItemUpdated,
                entityName: "ItemInventory",
                entityId: orderItem.ItemId,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Inventory restored +{request.RefundQuantity} for item {item.Name} (SKU: {item.SKU}) " +
                               $"to warehouse {inventory.Name} due to refund of {request.SerialNumber}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "Order item {SerialNumber} refunded successfully. " +
                "Quantity: {RefundQuantity}, Item: {ItemId}, " +
                "Inventory: {InventoryName} ({InventoryId}), " +
                "Restored: {RestoredQuantity}",
                request.SerialNumber,
                request.RefundQuantity,
                item.Id,
                inventory.Name,
                inventory.Id,
                request.RefundQuantity
            );

            return Result.SuccessResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding order item with serial number: {SerialNumber}", request.SerialNumber);
            return Result.FailureResult("An error occurred while processing the refund", 500);
        }
    }
}
