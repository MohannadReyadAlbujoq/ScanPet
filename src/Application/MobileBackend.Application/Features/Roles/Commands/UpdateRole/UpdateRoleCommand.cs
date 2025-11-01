using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Command to update an existing role
/// </summary>
public class UpdateRoleCommand : IRequest<Result<bool>>
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
