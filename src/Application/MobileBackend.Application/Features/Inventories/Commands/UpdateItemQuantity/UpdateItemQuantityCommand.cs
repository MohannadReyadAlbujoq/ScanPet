using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.UpdateItemQuantity;

/// <summary>
/// Command to update (set) item quantity at a specific inventory.
/// Uses Serializable transaction isolation via TransactionBehavior.
/// </summary>
public class UpdateItemQuantityCommand : IRequest<Result<ItemInventoryDto>>
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public string? Notes { get; set; }
}
