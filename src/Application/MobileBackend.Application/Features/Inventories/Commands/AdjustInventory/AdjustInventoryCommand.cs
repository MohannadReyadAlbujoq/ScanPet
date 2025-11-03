using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.AdjustInventory;

/// <summary>
/// Command to adjust inventory quantity (add or remove stock)
/// </summary>
public class AdjustInventoryCommand : IRequest<Result<ItemInventoryDto>>
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int QuantityChange { get; set; } // Positive = add, Negative = remove
    public string? Notes { get; set; }
    public string? Reason { get; set; }
}
