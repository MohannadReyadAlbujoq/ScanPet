using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Command to delete a role (soft delete)
/// </summary>
public class DeleteRoleCommand : IRequest<Result<bool>>
{
    public Guid RoleId { get; set; }
}
