using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.CreateInventory;

/// <summary>
/// Command to create a new inventory/warehouse/section
/// </summary>
public class CreateInventoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional parent Location ID — makes this inventory a section within a Location
    /// </summary>
    public Guid? LocationId { get; set; }
}
