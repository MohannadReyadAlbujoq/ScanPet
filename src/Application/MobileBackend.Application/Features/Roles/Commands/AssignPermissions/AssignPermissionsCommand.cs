using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Roles.Commands.AssignPermissions;

/// <summary>
/// Command to assign permissions to a role using bitwise operations
/// </summary>
public class AssignPermissionsCommand : IRequest<Result<bool>>
{
    public Guid RoleId { get; set; }
    public List<PermissionType> Permissions { get; set; } = new();
}
