using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Inventories.Commands.TransferInventory;

/// <summary>
/// Command to transfer inventory between warehouses
/// </summary>
public class TransferInventoryCommand : IRequest<Result<bool>>
{
    public Guid ItemId { get; set; }
    public Guid FromInventoryId { get; set; }
    public Guid ToInventoryId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public string? Reason { get; set; }
}
