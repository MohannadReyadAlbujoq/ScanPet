using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Queries.GetItemsInInventory;

/// <summary>
/// Handler for getting all items in a specific inventory
/// </summary>
public class GetItemsInInventoryQueryHandler : IRequestHandler<GetItemsInInventoryQuery, Result<List<ItemInventoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetItemsInInventoryQueryHandler> _logger;

    public GetItemsInInventoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetItemsInInventoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<ItemInventoryDto>>> Handle(GetItemsInInventoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify inventory exists
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inventory == null)
            {
                return Result<List<ItemInventoryDto>>.FailureResult($"Inventory with ID {request.InventoryId} not found", 404);
            }

            var itemInventories = await _unitOfWork.ItemInventories.GetByInventoryIdAsync(request.InventoryId, cancellationToken);

            var dtos = itemInventories.Select(ii => new ItemInventoryDto
            {
                Id = ii.Id,
                ItemId = ii.ItemId,
                ItemName = ii.Item?.Name ?? "Unknown",
                ItemSKU = ii.Item?.SKU,
                InventoryId = ii.InventoryId,
                InventoryName = ii.Inventory?.Name ?? "Unknown",
                Quantity = ii.Quantity,
                MinimumQuantity = ii.MinimumQuantity,
                MaximumQuantity = ii.MaximumQuantity,
                Notes = ii.Notes,
                IsLowStock = ii.MinimumQuantity.HasValue && ii.Quantity <= ii.MinimumQuantity.Value,
                CreatedAt = ii.CreatedAt,
                UpdatedAt = ii.UpdatedAt
            }).ToList();

            return Result<List<ItemInventoryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for inventory {InventoryId}", request.InventoryId);
            return Result<List<ItemInventoryDto>>.FailureResult("An error occurred while retrieving items", 500);
        }
    }
}
