using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Commands.SetItemInventory;

/// <summary>
/// Handler for setting/updating item inventory at a warehouse
/// </summary>
public class SetItemInventoryCommandHandler : IRequestHandler<SetItemInventoryCommand, Result<ItemInventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetItemInventoryCommandHandler> _logger;

    public SetItemInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<SetItemInventoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ItemInventoryDto>> Handle(SetItemInventoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
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

            // Validate quantity
            if (request.Quantity < 0)
            {
                return Result<ItemInventoryDto>.FailureResult("Quantity cannot be negative", 400);
            }

            // Check if item inventory already exists
            var existingItemInventory = await _unitOfWork.ItemInventories.GetByItemAndInventoryAsync(request.ItemId, request.InventoryId, cancellationToken);

            ItemInventory itemInventory;
            bool isNew = false;

            if (existingItemInventory != null)
            {
                // Update existing
                existingItemInventory.Quantity = request.Quantity;
                existingItemInventory.MinimumQuantity = request.MinimumQuantity;
                existingItemInventory.MaximumQuantity = request.MaximumQuantity;
                existingItemInventory.Notes = request.Notes;
                existingItemInventory.UpdatedAt = DateTime.UtcNow;
                existingItemInventory.UpdatedBy = _currentUserService.UserId;

                _unitOfWork.ItemInventories.Update(existingItemInventory);
                itemInventory = existingItemInventory;

                _logger.LogInformation("Updated item inventory: Item {ItemName} at {InventoryName}", item.Name, inventory.Name);
            }
            else
            {
                // Create new
                itemInventory = new ItemInventory
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

                await _unitOfWork.ItemInventories.AddAsync(itemInventory, cancellationToken);
                isNew = true;

                _logger.LogInformation("Created item inventory: Item {ItemName} at {InventoryName}", item.Name, inventory.Name);
            }

            // Audit log
            await _auditService.LogAsync(
                action: isNew ? AuditActions.Create : AuditActions.Update,
                entityName: "ItemInventory",
                entityId: itemInventory.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"{(isNew ? "Created" : "Updated")} item inventory: {item.Name} at {inventory.Name}, Quantity: {request.Quantity}",
                cancellationToken: cancellationToken
            );

            // Save all changes in single round-trip (TransactionBehavior handles commit)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new ItemInventoryDto
            {
                Id = itemInventory.Id,
                ItemId = request.ItemId,
                ItemName = item.Name,
                ItemSKU = item.SKU,
                InventoryId = request.InventoryId,
                InventoryName = inventory.Name,
                Quantity = request.Quantity,
                MinimumQuantity = request.MinimumQuantity,
                MaximumQuantity = request.MaximumQuantity,
                Notes = request.Notes,
                IsLowStock = request.MinimumQuantity.HasValue && request.Quantity <= request.MinimumQuantity.Value,
                CreatedAt = itemInventory.CreatedAt,
                UpdatedAt = itemInventory.UpdatedAt
            };

            return Result<ItemInventoryDto>.SuccessResult(dto, isNew ? 201 : 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting item inventory for Item {ItemId} at Inventory {InventoryId}", request.ItemId, request.InventoryId);
            return Result<ItemInventoryDto>.FailureResult("An error occurred while setting item inventory", 500);
        }
    }
}
