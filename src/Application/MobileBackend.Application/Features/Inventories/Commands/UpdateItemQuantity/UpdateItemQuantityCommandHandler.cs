using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Commands.UpdateItemQuantity;

/// <summary>
/// Handler for updating item quantity at a specific inventory.
/// Wrapped in Serializable transaction by TransactionBehavior for full ACID compliance:
/// - Atomicity: all changes commit or none
/// - Consistency: quantity cannot go negative
/// - Isolation: Serializable prevents dirty/non-repeatable/phantom reads and lost updates
/// - Durability: committed data persists
/// </summary>
public class UpdateItemQuantityCommandHandler : IRequestHandler<UpdateItemQuantityCommand, Result<ItemInventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateItemQuantityCommandHandler> _logger;

    public UpdateItemQuantityCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<UpdateItemQuantityCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ItemInventoryDto>> Handle(UpdateItemQuantityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate item exists
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
                return Result<ItemInventoryDto>.FailureResult($"Item with ID {request.ItemId} not found", 404);

            // Validate inventory exists
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inventory == null)
                return Result<ItemInventoryDto>.FailureResult($"Inventory with ID {request.InventoryId} not found", 404);

            if (!inventory.IsActive)
                return Result<ItemInventoryDto>.FailureResult($"Inventory '{inventory.Name}' is not active", 400);

            // Validate quantity
            if (request.Quantity < 0)
                return Result<ItemInventoryDto>.FailureResult("Quantity cannot be negative", 400);

            // Get or create item inventory record
            var existingItemInventory = await _unitOfWork.ItemInventories
                .GetByItemAndInventoryAsync(request.ItemId, request.InventoryId, cancellationToken);

            int oldQuantity = 0;
            bool isNew = false;

            if (existingItemInventory != null)
            {
                oldQuantity = existingItemInventory.Quantity;
                existingItemInventory.Quantity = request.Quantity;
                existingItemInventory.MinimumQuantity = request.MinimumQuantity ?? existingItemInventory.MinimumQuantity;
                existingItemInventory.MaximumQuantity = request.MaximumQuantity ?? existingItemInventory.MaximumQuantity;
                existingItemInventory.Notes = request.Notes ?? existingItemInventory.Notes;
                existingItemInventory.UpdatedAt = DateTime.UtcNow;
                existingItemInventory.UpdatedBy = _currentUserService.UserId;

                _unitOfWork.ItemInventories.Update(existingItemInventory);
            }
            else
            {
                isNew = true;
                existingItemInventory = new Domain.Entities.ItemInventory
                {
                    ItemId = request.ItemId,
                    InventoryId = request.InventoryId,
                    Quantity = request.Quantity,
                    MinimumQuantity = request.MinimumQuantity,
                    MaximumQuantity = request.MaximumQuantity,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.UserId
                };

                await _unitOfWork.ItemInventories.AddAsync(existingItemInventory, cancellationToken);
            }

            // Audit
            await _auditService.LogAsync(
                action: isNew ? AuditActions.Create : AuditActions.Update,
                entityName: "ItemInventory",
                entityId: existingItemInventory.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Quantity {(isNew ? "set" : $"changed from {oldQuantity}")} to {request.Quantity} for {item.Name} at {inventory.Name}",
                cancellationToken: cancellationToken
            );

            // Save all changes in single round-trip (TransactionBehavior handles commit)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Item inventory quantity updated: {ItemName} at {InventoryName}, {OldQty} -> {NewQty}",
                item.Name, inventory.Name, oldQuantity, request.Quantity);

            // Build warnings
            var warnings = new List<string>();
            if (existingItemInventory.MinimumQuantity.HasValue && request.Quantity <= existingItemInventory.MinimumQuantity.Value)
                warnings.Add($"Warning: Quantity ({request.Quantity}) is at or below minimum threshold ({existingItemInventory.MinimumQuantity.Value})");
            if (existingItemInventory.MaximumQuantity.HasValue && request.Quantity >= existingItemInventory.MaximumQuantity.Value)
                warnings.Add($"Warning: Quantity ({request.Quantity}) is at or above maximum capacity ({existingItemInventory.MaximumQuantity.Value})");

            var dto = new ItemInventoryDto
            {
                Id = existingItemInventory.Id,
                ItemId = request.ItemId,
                ItemName = item.Name,
                ItemSKU = item.SKU,
                InventoryId = request.InventoryId,
                InventoryName = inventory.Name,
                Quantity = request.Quantity,
                MinimumQuantity = existingItemInventory.MinimumQuantity,
                MaximumQuantity = existingItemInventory.MaximumQuantity,
                Notes = warnings.Count > 0 ? string.Join("; ", warnings) : existingItemInventory.Notes,
                IsLowStock = existingItemInventory.MinimumQuantity.HasValue && request.Quantity <= existingItemInventory.MinimumQuantity.Value,
                CreatedAt = existingItemInventory.CreatedAt,
                UpdatedAt = existingItemInventory.UpdatedAt
            };

            return Result<ItemInventoryDto>.SuccessResult(dto, isNew ? 201 : 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item quantity for Item {ItemId} at Inventory {InventoryId}", request.ItemId, request.InventoryId);
            return Result<ItemInventoryDto>.FailureResult("An error occurred while updating item quantity", 500);
        }
    }
}
