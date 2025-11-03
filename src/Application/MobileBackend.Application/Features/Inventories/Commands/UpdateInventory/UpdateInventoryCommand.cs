using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Inventories.Commands.UpdateInventory;

/// <summary>
/// Command to update an inventory/warehouse
/// </summary>
public class UpdateInventoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
