using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Queries.GetInventoryItemCounts;

// ?????????????????????????????????????????????????????????????
// Handler: GET /api/inventories/item-counts  (all inventories)
// Returns global totals + per-inventory breakdown
// ?????????????????????????????????????????????????????????????
public class GetAllInventoriesItemCountsQueryHandler
    : IRequestHandler<GetAllInventoriesItemCountsQuery, Result<AllInventoriesItemCountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllInventoriesItemCountsQueryHandler> _logger;

    public GetAllInventoriesItemCountsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllInventoriesItemCountsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AllInventoriesItemCountDto>> Handle(
        GetAllInventoriesItemCountsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var inventories = (await _unitOfWork.Inventories
                .FindAsync(i => !i.IsDeleted, cancellationToken))
                .ToList();

            var perInventory = new List<InventoryItemCountDto>();

            // Collect every ItemInventory row across all inventories to compute globals
            // key = ItemId, value = accumulated data
            var globalMap = new Dictionary<Guid, (string Name, string? SKU, int Qty, int InvCount, bool LowAnywhere)>();

            foreach (var inv in inventories)
            {
                var rows = await _unitOfWork.ItemInventories
                    .GetByInventoryIdAsync(inv.Id, cancellationToken);

                var items = rows
                    .Select(ii => new ItemCountEntryDto
                    {
                        ItemId          = ii.ItemId,
                        ItemName        = ii.Item?.Name ?? "Unknown",
                        ItemSKU         = ii.Item?.SKU,
                        Quantity        = ii.Quantity,
                        MinimumQuantity = ii.MinimumQuantity,
                        IsLowStock      = ii.MinimumQuantity.HasValue && ii.Quantity <= ii.MinimumQuantity.Value
                    })
                    .OrderByDescending(x => x.Quantity)
                    .ToList();

                perInventory.Add(new InventoryItemCountDto
                {
                    InventoryId    = inv.Id,
                    InventoryName  = inv.Name,
                    IsActive       = inv.IsActive,
                    TotalItemTypes = items.Count,
                    TotalQuantity  = items.Sum(i => i.Quantity),
                    Items          = items
                });

                // Accumulate into global map
                foreach (var entry in items)
                {
                    if (globalMap.TryGetValue(entry.ItemId, out var existing))
                    {
                        globalMap[entry.ItemId] = (
                            existing.Name,
                            existing.SKU,
                            existing.Qty + entry.Quantity,
                            existing.InvCount + 1,
                            existing.LowAnywhere || entry.IsLowStock
                        );
                    }
                    else
                    {
                        globalMap[entry.ItemId] = (
                            entry.ItemName,
                            entry.ItemSKU,
                            entry.Quantity,
                            1,
                            entry.IsLowStock
                        );
                    }
                }
            }

            var globalTotals = globalMap
                .Select(kv => new GlobalItemTotalDto
                {
                    ItemId            = kv.Key,
                    ItemName          = kv.Value.Name,
                    ItemSKU           = kv.Value.SKU,
                    TotalQuantity     = kv.Value.Qty,
                    InventoryCount    = kv.Value.InvCount,
                    IsLowStockAnywhere = kv.Value.LowAnywhere
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            var response = new AllInventoriesItemCountDto
            {
                GlobalTotalItemTypes = globalMap.Count,
                GlobalTotalQuantity  = globalMap.Values.Sum(v => v.Qty),
                GlobalItemTotals     = globalTotals,
                Inventories          = perInventory
            };

            return Result<AllInventoriesItemCountDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all-inventories item counts");
            return Result<AllInventoriesItemCountDto>.FailureResult(
                "An error occurred while retrieving inventory item counts", 500);
        }
    }
}

// ?????????????????????????????????????????????????????????????
// Handler: GET /api/inventories/{inventoryId}/item-counts
// Returns breakdown for a single inventory
// ?????????????????????????????????????????????????????????????
public class GetSingleInventoryItemCountsQueryHandler
    : IRequestHandler<GetSingleInventoryItemCountsQuery, Result<InventoryItemCountDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSingleInventoryItemCountsQueryHandler> _logger;

    public GetSingleInventoryItemCountsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetSingleInventoryItemCountsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<InventoryItemCountDto>> Handle(
        GetSingleInventoryItemCountsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var inv = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inv == null)
                return Result<InventoryItemCountDto>.FailureResult(
                    $"Inventory with ID {request.InventoryId} not found", 404);

            var rows = await _unitOfWork.ItemInventories
                .GetByInventoryIdAsync(inv.Id, cancellationToken);

            var items = rows
                .Select(ii => new ItemCountEntryDto
                {
                    ItemId          = ii.ItemId,
                    ItemName        = ii.Item?.Name ?? "Unknown",
                    ItemSKU         = ii.Item?.SKU,
                    Quantity        = ii.Quantity,
                    MinimumQuantity = ii.MinimumQuantity,
                    IsLowStock      = ii.MinimumQuantity.HasValue && ii.Quantity <= ii.MinimumQuantity.Value
                })
                .OrderByDescending(x => x.Quantity)
                .ToList();

            var dto = new InventoryItemCountDto
            {
                InventoryId    = inv.Id,
                InventoryName  = inv.Name,
                IsActive       = inv.IsActive,
                TotalItemTypes = items.Count,
                TotalQuantity  = items.Sum(i => i.Quantity),
                Items          = items
            };

            return Result<InventoryItemCountDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item counts for inventory {InventoryId}", request.InventoryId);
            return Result<InventoryItemCountDto>.FailureResult(
                "An error occurred while retrieving inventory item counts", 500);
        }
    }
}
