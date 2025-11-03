using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.UpdateUserRole;

/// <summary>
/// Command to update user's role
/// </summary>
public class UpdateUserRoleCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
