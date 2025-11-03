using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Queries.GetItemsInInventory;

/// <summary>
/// Query to get all items in a specific inventory
/// </summary>
public class GetItemsInInventoryQuery : IRequest<Result<List<ItemInventoryDto>>>
{
    public Guid InventoryId { get; set; }
}
