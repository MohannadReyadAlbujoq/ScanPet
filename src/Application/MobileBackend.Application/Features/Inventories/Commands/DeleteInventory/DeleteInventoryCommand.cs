using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Inventories.Commands.DeleteInventory;

/// <summary>
/// Command to delete (soft delete) an inventory/warehouse
/// </summary>
public class DeleteInventoryCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
