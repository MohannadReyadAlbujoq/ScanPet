using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;

namespace MobileBackend.Application.Features.Roles.Queries.SearchRoles;

/// <summary>
/// Query to search roles by name or description
/// Inherits pagination and search term from BaseSearchQuery
/// </summary>
public class SearchRolesQuery : BaseSearchQuery<RoleDto>, IRequest<Result<List<RoleDto>>>
{
}
