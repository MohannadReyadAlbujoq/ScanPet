using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.IncrementInventory;

/// <summary>
/// Command to increment item quantity at a specific inventory.
/// Uses Serializable transaction isolation via TransactionBehavior.
/// Returns warning if quantity exceeds maximum threshold.
/// </summary>
public class IncrementInventoryCommand : IRequest<Result<ItemInventoryDto>>
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
    public string? Reason { get; set; }
}
