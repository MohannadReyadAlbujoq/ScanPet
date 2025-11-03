using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;

namespace MobileBackend.Application.Features.Roles.Queries.GetAllRoles;

/// <summary>
/// Query to get all roles with optional pagination
/// </summary>
public class GetAllRolesQuery : BasePagedQuery<RoleDto>
{
}
