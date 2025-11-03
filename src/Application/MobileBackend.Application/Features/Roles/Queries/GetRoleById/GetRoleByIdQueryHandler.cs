using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Roles;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Roles.Queries.GetRoleById;

/// <summary>
/// Handler for GetRoleByIdQuery
/// Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetRoleByIdQueryHandler : BaseGetByIdHandler<GetRoleByIdQuery, Role, RoleDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetRoleByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Role?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Roles.GetByIdWithPermissionsAsync(id, cancellationToken);
    }

    protected override RoleDto MapToDto(Role entity)
    {
        var rolePermission = entity.RolePermissions.FirstOrDefault();
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
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            PermissionsBitmask = permissionsBitmask,
            Permissions = permissions,
            UserCount = entity.UserRoles.Count,
            CreatedAt = entity.CreatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Role;
}
