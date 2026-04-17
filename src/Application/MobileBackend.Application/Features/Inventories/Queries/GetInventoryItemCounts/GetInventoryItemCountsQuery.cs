using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetInventoryItemCounts;

/// <summary>
/// Query to get item counts per inventory.
/// If InventoryId is provided, returns counts for that single inventory only.
/// Otherwise returns counts for all inventories.
/// </summary>
public class GetInventoryItemCountsQuery : IRequest<Result<List<InventoryItemCountDto>>>
{
    /// <summary>
    /// Optional: filter to a single inventory
    /// </summary>
    public Guid? InventoryId { get; set; }
}
