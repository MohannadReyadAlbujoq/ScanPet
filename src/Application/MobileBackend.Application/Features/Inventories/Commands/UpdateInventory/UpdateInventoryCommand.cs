using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Inventories.Commands.UpdateInventory;

/// <summary>
/// Command to update an inventory/warehouse/section
/// </summary>
public class UpdateInventoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Optional parent Location ID — makes this inventory a section within a Location
    /// </summary>
    public Guid? LocationId { get; set; }
}
