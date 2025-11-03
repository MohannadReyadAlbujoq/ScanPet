using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Queries.GetLowStockItems;

/// <summary>
/// Handler for getting items with low stock
/// </summary>
public class GetLowStockItemsQueryHandler : IRequestHandler<GetLowStockItemsQuery, Result<List<ItemInventoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetLowStockItemsQueryHandler> _logger;

    public GetLowStockItemsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetLowStockItemsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<ItemInventoryDto>>> Handle(GetLowStockItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var lowStockItems = await _unitOfWork.ItemInventories.GetLowStockItemsAsync(cancellationToken);

            var dtos = lowStockItems.Select(ii => new ItemInventoryDto
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
                IsLowStock = true, // All items in this list are low stock
                CreatedAt = ii.CreatedAt,
                UpdatedAt = ii.UpdatedAt
            }).ToList();

            return Result<List<ItemInventoryDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock items");
            return Result<List<ItemInventoryDto>>.FailureResult("An error occurred while retrieving low stock items", 500);
        }
    }
}
