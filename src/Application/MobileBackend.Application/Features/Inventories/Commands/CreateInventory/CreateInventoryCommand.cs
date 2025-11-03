using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;

namespace MobileBackend.Application.Features.Inventories.Commands.CreateInventory;

/// <summary>
/// Command to create a new inventory/warehouse
/// </summary>
public class CreateInventoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
