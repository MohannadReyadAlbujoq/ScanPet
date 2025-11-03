using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Commands.AdjustInventory;

/// <summary>
/// Handler for adjusting inventory quantity
/// </summary>
public class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand, Result<ItemInventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdjustInventoryCommandHandler> _logger;

    public AdjustInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<AdjustInventoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ItemInventoryDto>> Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate quantity change is not zero
            if (request.QuantityChange == 0)
            {
                return Result<ItemInventoryDto>.FailureResult("Quantity change cannot be zero", 400);
            }

            // Validate item exists
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result<ItemInventoryDto>.FailureResult($"Item with ID {request.ItemId} not found", 404);
            }

            // Validate inventory exists
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inventory == null)
            {
                return Result<ItemInventoryDto>.FailureResult($"Inventory with ID {request.InventoryId} not found", 404);
            }

            // Validate inventory is active
            if (!inventory.IsActive)
            {
                return Result<ItemInventoryDto>.FailureResult($"Inventory '{inventory.Name}' is not active", 400);
            }

            // Use repository method to adjust inventory
            var success = await _unitOfWork.ItemInventories.AdjustInventoryAsync(
                request.ItemId,
                request.InventoryId,
                request.QuantityChange,
                cancellationToken
            );

            if (!success)
            {
                return Result<ItemInventoryDto>.FailureResult("Failed to adjust inventory. Insufficient stock or invalid operation.", 400);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get updated item inventory
            var itemInventory = await _unitOfWork.ItemInventories.GetByItemAndInventoryAsync(request.ItemId, request.InventoryId, cancellationToken);

            if (itemInventory == null)
            {
                return Result<ItemInventoryDto>.FailureResult("Item inventory not found after adjustment", 500);
            }

            // Audit log
            var adjustmentType = request.QuantityChange > 0 ? "Added" : "Removed";
            var auditInfo = $"{adjustmentType} {Math.Abs(request.QuantityChange)} units of {item.Name} at {inventory.Name}";
            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                auditInfo += $" - Reason: {request.Reason}";
            }

            await _auditService.LogAsync(
                action: AuditActions.Update,
                entityName: "ItemInventory",
                entityId: itemInventory.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: auditInfo,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Inventory adjusted: {AdjustmentType} {Quantity} of {ItemName} at {InventoryName}",
                adjustmentType, Math.Abs(request.QuantityChange), item.Name, inventory.Name);

            var dto = new ItemInventoryDto
            {
                Id = itemInventory.Id,
                ItemId = itemInventory.ItemId,
                ItemName = item.Name,
                ItemSKU = item.SKU,
                InventoryId = itemInventory.InventoryId,
                InventoryName = inventory.Name,
                Quantity = itemInventory.Quantity,
                MinimumQuantity = itemInventory.MinimumQuantity,
                MaximumQuantity = itemInventory.MaximumQuantity,
                Notes = request.Notes ?? itemInventory.Notes,
                IsLowStock = itemInventory.MinimumQuantity.HasValue && itemInventory.Quantity <= itemInventory.MinimumQuantity.Value,
                CreatedAt = itemInventory.CreatedAt,
                UpdatedAt = itemInventory.UpdatedAt
            };

            return Result<ItemInventoryDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting inventory for Item {ItemId} at Inventory {InventoryId}", request.ItemId, request.InventoryId);
            return Result<ItemInventoryDto>.FailureResult("An error occurred while adjusting inventory", 500);
        }
    }
}
