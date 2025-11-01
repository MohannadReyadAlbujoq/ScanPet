using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Roles.Queries.GetAllRoles;

/// <summary>
/// Handler for GetAllRolesQuery
/// </summary>
public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<RoleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRolesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllWithPermissionsAsync(cancellationToken);

        var roleDtos = roles.Select(role =>
        {
            var rolePermission = role.RolePermissions.FirstOrDefault();
            var permissionsBitmask = rolePermission?.PermissionsBitmask ?? 0L;
            
            // Convert bitmask to permission names
            var permissions = new List<string>();
            foreach (PermissionType permission in Enum.GetValues(typeof(PermissionType)))
            {
                if ((permissionsBitmask & (long)permission) != 0)
                {
                    permissions.Add(permission.ToString());
                }
            }

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                PermissionsBitmask = permissionsBitmask,
                Permissions = permissions,
                UserCount = role.UserRoles.Count,
                CreatedAt = role.CreatedAt
            };
        }).ToList();

        return Result<List<RoleDto>>.SuccessResult(roleDtos);
    }
}
