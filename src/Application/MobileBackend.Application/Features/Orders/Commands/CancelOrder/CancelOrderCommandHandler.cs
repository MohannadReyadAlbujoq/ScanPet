using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Handler for cancelling an order and restoring item quantities
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetWithItemsAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                return Result<bool>.FailureResult("Order not found", 404);
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                return Result<bool>.FailureResult("Order is already cancelled", 400);
            }

            // ? FIX N+1: Fetch all items in single query before loop
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                var itemIds = order.OrderItems.Select(oi => oi.ItemId).Distinct().ToList();
                var items = await _unitOfWork.Items.FindAsync(
                    i => itemIds.Contains(i.Id), 
                    cancellationToken);
                
                var itemsDict = items.ToDictionary(i => i.Id);

                // Restore item quantities to inventories
                foreach (var orderItem in order.OrderItems)
                {
                    if (itemsDict.TryGetValue(orderItem.ItemId, out var item))
                    {
                        // Restore to the first available active inventory for this item
                        var inventories = await _unitOfWork.ItemInventories.GetByItemIdAsync(orderItem.ItemId, cancellationToken);
                        var targetInventory = inventories.FirstOrDefault(i => i.Inventory != null && i.Inventory.IsActive);
                        
                        if (targetInventory != null)
                        {
                            targetInventory.Quantity += orderItem.Quantity;
                            targetInventory.UpdatedAt = DateTime.UtcNow;
                            targetInventory.UpdatedBy = _currentUserService.UserId;
                            _unitOfWork.ItemInventories.Update(targetInventory);
                        }
                        else
                        {
                            _logger.LogWarning("No active inventory found for item {ItemId} when cancelling order {OrderId}",
                                orderItem.ItemId, order.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Item {ItemId} not found when cancelling order {OrderId}", 
                            orderItem.ItemId, order.Id);
                    }
                }
            }

            order.OrderStatus = OrderStatus.Cancelled;
            order.Description = string.IsNullOrEmpty(order.Description) 
                ? $"Cancelled: {request.CancellationReason}" 
                : $"{order.Description}\nCancelled: {request.CancellationReason}";
            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.OrderCancelled,
                entityName: EntityNames.Order,
                entityId: order.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Order {order.OrderNumber} cancelled. Reason: {request.CancellationReason}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Order cancelled successfully: {OrderId} - {OrderNumber}", order.Id, order.OrderNumber);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order: {OrderId}", request.OrderId);
            return Result<bool>.FailureResult("An error occurred while cancelling the order", 500);
        }
    }
}
