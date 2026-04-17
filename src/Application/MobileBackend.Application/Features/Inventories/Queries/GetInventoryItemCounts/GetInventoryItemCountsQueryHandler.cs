using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Queries.GetInventoryItemCounts;

/// <summary>
/// Handler: returns item quantity counts grouped by inventory.
/// Uses a single DB call per inventory leveraging the existing
/// IItemInventoryRepository.GetByInventoryIdAsync method.
/// Placed on the Inventories controller as GET /api/inventories/item-counts
/// (or GET /api/inventories/{id}/item-counts for a single inventory).
/// </summary>
public class GetInventoryItemCountsQueryHandler
    : IRequestHandler<GetInventoryItemCountsQuery, Result<List<InventoryItemCountDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetInventoryItemCountsQueryHandler> _logger;

    public GetInventoryItemCountsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetInventoryItemCountsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<InventoryItemCountDto>>> Handle(
        GetInventoryItemCountsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Determine which inventories to summarise
            List<Domain.Entities.Inventory> inventories;

            if (request.InventoryId.HasValue)
            {
                var single = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId.Value, cancellationToken);
                if (single == null)
                    return Result<List<InventoryItemCountDto>>.FailureResult(
                        $"Inventory with ID {request.InventoryId.Value} not found", 404);

                inventories = new List<Domain.Entities.Inventory> { single };
            }
            else
            {
                // GetAllAsync from generic repo (no N+1 — items loaded per-inventory below)
                var all = await _unitOfWork.Inventories.FindAsync(
                    i => !i.IsDeleted, cancellationToken);
                inventories = all.ToList();
            }

            var result = new List<InventoryItemCountDto>();

            foreach (var inv in inventories)
            {
                // Reuse existing repo method — already includes Item + Color
                var itemInventories = await _unitOfWork.ItemInventories
                    .GetByInventoryIdAsync(inv.Id, cancellationToken);

                var items = itemInventories
                    .Select(ii => new ItemCountEntryDto
                    {
                        ItemId = ii.ItemId,
                        ItemName = ii.Item?.Name ?? "Unknown",
                        ItemSKU = ii.Item?.SKU,
                        Quantity = ii.Quantity,
                        MinimumQuantity = ii.MinimumQuantity,
                        IsLowStock = ii.MinimumQuantity.HasValue && ii.Quantity <= ii.MinimumQuantity.Value
                    })
                    .OrderByDescending(x => x.Quantity)
                    .ToList();

                result.Add(new InventoryItemCountDto
                {
                    InventoryId = inv.Id,
                    InventoryName = inv.Name,
                    IsActive = inv.IsActive,
                    TotalItemTypes = items.Count,
                    TotalQuantity = items.Sum(i => i.Quantity),
                    Items = items
                });
            }

            return Result<List<InventoryItemCountDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory item counts");
            return Result<List<InventoryItemCountDto>>.FailureResult(
                "An error occurred while retrieving inventory item counts", 500);
        }
    }
}
