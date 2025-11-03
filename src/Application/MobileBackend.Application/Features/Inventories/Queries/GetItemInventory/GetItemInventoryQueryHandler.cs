using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Queries.GetItemInventory;

/// <summary>
/// Handler for getting item inventory across all warehouses
/// </summary>
public class GetItemInventoryQueryHandler : IRequestHandler<GetItemInventoryQuery, Result<List<ItemInventoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetItemInventoryQueryHandler> _logger;

    public GetItemInventoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetItemInventoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<ItemInventoryDto>>> Handle(GetItemInventoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify item exists
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result<List<ItemInventoryDto>>.FailureResult($"Item with ID {request.ItemId} not found", 404);
            }

            var itemInventories = await _unitOfWork.ItemInventories.GetByItemIdAsync(request.ItemId, cancellationToken);

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
            _logger.LogError(ex, "Error retrieving inventory for item {ItemId}", request.ItemId);
            return Result<List<ItemInventoryDto>>.FailureResult("An error occurred while retrieving item inventory", 500);
        }
    }
}
