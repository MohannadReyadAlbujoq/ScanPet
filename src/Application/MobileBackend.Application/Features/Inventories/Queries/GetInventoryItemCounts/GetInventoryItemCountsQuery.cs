using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetInventoryItemCounts;

/// <summary>
/// Query for GET /api/inventories/item-counts
/// Returns global totals + per-inventory breakdown.
/// </summary>
public class GetAllInventoriesItemCountsQuery : IRequest<Result<AllInventoriesItemCountDto>> { }

/// <summary>
/// Query for GET /api/inventories/{inventoryId}/item-counts
/// Returns count breakdown for a single inventory.
/// </summary>
public class GetSingleInventoryItemCountsQuery : IRequest<Result<InventoryItemCountDto>>
{
    public Guid InventoryId { get; set; }
}
