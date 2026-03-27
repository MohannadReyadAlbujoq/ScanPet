using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Commands.IncrementInventory;

/// <summary>
/// Handler for incrementing item quantity at a specific inventory.
/// Wrapped in Serializable transaction by TransactionBehavior.
/// Returns warning if new quantity exceeds maximum capacity.
/// </summary>
public class IncrementInventoryCommandHandler : IRequestHandler<IncrementInventoryCommand, Result<ItemInventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<IncrementInventoryCommandHandler> _logger;

    public IncrementInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<IncrementInventoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ItemInventoryDto>> Handle(IncrementInventoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Quantity <= 0)
                return Result<ItemInventoryDto>.FailureResult("Increment quantity must be greater than 0", 400);

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

            int oldQuantity = 0;
            bool isNew = false;

            if (itemInventory != null)
            {
                oldQuantity = itemInventory.Quantity;
                itemInventory.Quantity += request.Quantity;
                itemInventory.Notes = request.Notes ?? itemInventory.Notes;
                itemInventory.UpdatedAt = DateTime.UtcNow;
                itemInventory.UpdatedBy = _currentUserService.UserId;
                _unitOfWork.ItemInventories.Update(itemInventory);
            }
            else
            {
                isNew = true;
                itemInventory = new ItemInventory
                {
                    ItemId = request.ItemId,
                    InventoryId = request.InventoryId,
                    Quantity = request.Quantity,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.UserId
                };
                await _unitOfWork.ItemInventories.AddAsync(itemInventory, cancellationToken);
            }

            // Audit
            await _auditService.LogAsync(
                action: AuditActions.Update,
                entityName: "ItemInventory",
                entityId: itemInventory.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Incremented +{request.Quantity} for {item.Name} at {inventory.Name} ({oldQuantity} -> {itemInventory.Quantity})" +
                               (request.Reason != null ? $" - Reason: {request.Reason}" : ""),
                cancellationToken: cancellationToken
            );

            // Save all changes in single round-trip (TransactionBehavior handles commit)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inventory incremented: +{Qty} of {ItemName} at {InventoryName} ({OldQty} -> {NewQty})",
                request.Quantity, item.Name, inventory.Name, oldQuantity, itemInventory.Quantity);

            // Build warnings
            var warnings = new List<string>();
            if (itemInventory.MaximumQuantity.HasValue && itemInventory.Quantity > itemInventory.MaximumQuantity.Value)
                warnings.Add($"Warning: Quantity ({itemInventory.Quantity}) exceeds maximum capacity ({itemInventory.MaximumQuantity.Value})");

            var dto = new ItemInventoryDto
            {
                Id = itemInventory.Id,
                ItemId = request.ItemId,
                ItemName = item.Name,
                ItemSKU = item.SKU,
                InventoryId = request.InventoryId,
                InventoryName = inventory.Name,
                Quantity = itemInventory.Quantity,
                MinimumQuantity = itemInventory.MinimumQuantity,
                MaximumQuantity = itemInventory.MaximumQuantity,
                Notes = warnings.Count > 0 ? string.Join("; ", warnings) : itemInventory.Notes,
                IsLowStock = itemInventory.MinimumQuantity.HasValue && itemInventory.Quantity <= itemInventory.MinimumQuantity.Value,
                CreatedAt = itemInventory.CreatedAt,
                UpdatedAt = itemInventory.UpdatedAt
            };

            return Result<ItemInventoryDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing inventory for Item {ItemId} at Inventory {InventoryId}", request.ItemId, request.InventoryId);
            return Result<ItemInventoryDto>.FailureResult("An error occurred while incrementing inventory", 500);
        }
    }
}
