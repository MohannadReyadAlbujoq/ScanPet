using MediatR;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Roles.Commands.AssignPermissions;

/// <summary>
/// Handler for AssignPermissionsCommand
/// Uses bitwise operations for efficient permission management
/// </summary>
public class AssignPermissionsCommandHandler : IRequestHandler<AssignPermissionsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public AssignPermissionsCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<bool>> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result<bool>.FailureResult("Role not found", 404);
        }

        // Calculate permissions bitmask
        long permissionsBitmask = 0;
        foreach (var permission in request.Permissions)
        {
            permissionsBitmask |= (long)permission;
        }

        // Get or create RolePermission
        var rolePermission = await _unitOfWork.Permissions.GetRolePermissionAsync(request.RoleId, cancellationToken);
        
        if (rolePermission == null)
        {
            // Create new RolePermission
            rolePermission = new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = request.RoleId,
                PermissionsBitmask = permissionsBitmask,
                CreatedAt = _dateTimeService.UtcNow,
                CreatedBy = _currentUserService.UserId ?? Guid.Empty
            };
            await _unitOfWork.Permissions.AddRolePermissionAsync(rolePermission, cancellationToken);
        }
        else
        {
            // Update existing RolePermission
            rolePermission.PermissionsBitmask = permissionsBitmask;
            rolePermission.UpdatedAt = _dateTimeService.UtcNow;
            rolePermission.UpdatedBy = _currentUserService.UserId;
            _unitOfWork.Permissions.UpdateRolePermission(rolePermission);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        var permissionNames = string.Join(", ", request.Permissions.Select(p => p.ToString()));
        await _auditService.LogAsync(
            AuditActions.PermissionsAssigned,
            EntityNames.Role,
            role.Id,
            _currentUserService.UserId ?? Guid.Empty,
            $"Permissions assigned to role {role.Name}: {permissionNames}",
            cancellationToken);

        return Result<bool>.SuccessResult(true);
    }
}
