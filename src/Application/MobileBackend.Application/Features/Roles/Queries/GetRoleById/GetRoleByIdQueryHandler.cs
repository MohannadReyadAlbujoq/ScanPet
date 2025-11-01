using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Roles;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Handler for GetRoleByIdQuery
/// </summary>
public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result<RoleDto>.FailureResult("Role not found", 404);
        }

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

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            PermissionsBitmask = permissionsBitmask,
            Permissions = permissions,
            UserCount = role.UserRoles.Count,
            CreatedAt = role.CreatedAt
        };

        return Result<RoleDto>.SuccessResult(roleDto);
    }
}
