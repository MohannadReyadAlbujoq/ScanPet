using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.SetDefaultInventory;

/// <summary>
/// Command to set a user's default inventory
/// </summary>
public class SetDefaultInventoryCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public Guid? DefaultInventoryId { get; set; }
}
