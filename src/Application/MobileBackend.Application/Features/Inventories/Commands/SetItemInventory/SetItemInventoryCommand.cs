using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.SetItemInventory;

/// <summary>
/// Command to set or update item inventory at a warehouse
/// </summary>
public class SetItemInventoryCommand : IRequest<Result<ItemInventoryDto>>
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public string? Notes { get; set; }
}
