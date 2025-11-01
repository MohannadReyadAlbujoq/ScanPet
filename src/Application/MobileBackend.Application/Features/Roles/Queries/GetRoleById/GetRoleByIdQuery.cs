using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;

namespace MobileBackend.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Query to get role by ID with permissions
/// </summary>
public class GetRoleByIdQuery : IRequest<Result<RoleDto>>
{
    public Guid RoleId { get; set; }
}
