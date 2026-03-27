using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Commands.DecrementInventory;

/// <summary>
/// Handler for decrementing item quantity at a specific inventory.
/// Wrapped in Serializable transaction by TransactionBehavior.
/// - Error if result would go below 0
/// - Warning if result falls below minimum threshold
/// </summary>
public class DecrementInventoryCommandHandler : IRequestHandler<DecrementInventoryCommand, Result<ItemInventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DecrementInventoryCommandHandler> _logger;

    public DecrementInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<DecrementInventoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ItemInventoryDto>> Handle(DecrementInventoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Quantity <= 0)
                return Result<ItemInventoryDto>.FailureResult("Decrement quantity must be greater than 0", 400);

            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
                return Result<ItemInventoryDto>.FailureResult($"Item with ID {request.ItemId} not found", 404);

            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inventory == null)
                return Result<ItemInventoryDto>.FailureResult($"Inventory with ID {request.InventoryId} not found", 404);

            if (!inventory.IsActive)
                return Result<ItemInventoryDto>.FailureResult($"Inventory '{inventory.Name}' is not active", 400);

            var itemInventory = await _unitOfWork.ItemInventories
                .GetByItemAndInventoryAsync(request.ItemId, request.InventoryId, cancellationToken);

            if (itemInventory == null)
                return Result<ItemInventoryDto>.FailureResult(
                    $"Item '{item.Name}' does not exist in inventory '{inventory.Name}'", 404);

            int oldQuantity = itemInventory.Quantity;
            int newQuantity = oldQuantity - request.Quantity;

            // Error if result would go below 0
            if (newQuantity < 0)
            {
                return Result<ItemInventoryDto>.FailureResult(
                    $"Cannot decrement by {request.Quantity}. Current quantity is {oldQuantity}. " +
                    $"Result would be {newQuantity} which is below 0.", 400);
            }

            itemInventory.Quantity = newQuantity;
            itemInventory.Notes = request.Notes ?? itemInventory.Notes;
            itemInventory.UpdatedAt = DateTime.UtcNow;
            itemInventory.UpdatedBy = _currentUserService.UserId;
            _unitOfWork.ItemInventories.Update(itemInventory);

            // Audit
            await _auditService.LogAsync(
                action: AuditActions.Update,
                entityName: "ItemInventory",
                entityId: itemInventory.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Decremented -{request.Quantity} for {item.Name} at {inventory.Name} ({oldQuantity} -> {newQuantity})" +
                               (request.Reason != null ? $" - Reason: {request.Reason}" : ""),
                cancellationToken: cancellationToken
            );

            // Save all changes in single round-trip (TransactionBehavior handles commit)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inventory decremented: -{Qty} of {ItemName} at {InventoryName} ({OldQty} -> {NewQty})",
                request.Quantity, item.Name, inventory.Name, oldQuantity, newQuantity);

            // Build warnings
            var warnings = new List<string>();
            if (itemInventory.MinimumQuantity.HasValue && newQuantity < itemInventory.MinimumQuantity.Value)
                warnings.Add($"Warning: Quantity ({newQuantity}) is below minimum threshold ({itemInventory.MinimumQuantity.Value})");
            if (newQuantity == 0)
                warnings.Add("Warning: Item is now out of stock at this inventory");

            var dto = new ItemInventoryDto
            {
                Id = itemInventory.Id,
                ItemId = request.ItemId,
                ItemName = item.Name,
                ItemSKU = item.SKU,
                InventoryId = request.InventoryId,
                InventoryName = inventory.Name,
                Quantity = newQuantity,
                MinimumQuantity = itemInventory.MinimumQuantity,
                MaximumQuantity = itemInventory.MaximumQuantity,
                Notes = warnings.Count > 0 ? string.Join("; ", warnings) : itemInventory.Notes,
                IsLowStock = itemInventory.MinimumQuantity.HasValue && newQuantity <= itemInventory.MinimumQuantity.Value,
                CreatedAt = itemInventory.CreatedAt,
                UpdatedAt = itemInventory.UpdatedAt
            };

            return Result<ItemInventoryDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing inventory for Item {ItemId} at Inventory {InventoryId}", request.ItemId, request.InventoryId);
            return Result<ItemInventoryDto>.FailureResult("An error occurred while decrementing inventory", 500);
        }
    }
}
