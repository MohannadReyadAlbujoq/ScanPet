using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.DecrementInventory;

/// <summary>
/// Command to decrement item quantity at a specific inventory.
/// Uses Serializable transaction isolation via TransactionBehavior.
/// Returns error if quantity would go below 0.
/// Returns warning if quantity falls below minimum threshold.
/// </summary>
public class DecrementInventoryCommand : IRequest<Result<ItemInventoryDto>>
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
    public string? Reason { get; set; }
}
