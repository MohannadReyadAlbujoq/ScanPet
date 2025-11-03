using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetItemInventory;

/// <summary>
/// Query to get item inventory across all warehouses
/// </summary>
public class GetItemInventoryQuery : IRequest<Result<List<ItemInventoryDto>>>
{
    public Guid ItemId { get; set; }
}
