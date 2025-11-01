using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;

namespace MobileBackend.Application.Features.Roles.Queries.GetAllRoles;

/// <summary>
/// Query to get all roles
/// </summary>
public class GetAllRolesQuery : IRequest<Result<List<RoleDto>>>
{
}
