using MediatR;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Handler for DeleteRoleCommand
/// Performs soft delete
/// </summary>
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result<bool>.FailureResult("Role not found", 404);
        }

        // Check if role is assigned to any users
        var usersWithRole = await _unitOfWork.Roles.GetUserCountAsync(request.RoleId, cancellationToken);
        if (usersWithRole > 0)
        {
            return Result<bool>.FailureResult(
                $"Cannot delete role. It is assigned to {usersWithRole} user(s)", 
                400);
        }

        // Soft delete
        var deleted = await _unitOfWork.Roles.SoftDeleteAsync(
            request.RoleId, 
            _currentUserService.UserId ?? Guid.Empty, 
            cancellationToken);

        if (!deleted)
        {
            return Result<bool>.FailureResult("Failed to delete role", 500);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogAsync(
            AuditActions.RoleDeleted,
            EntityNames.Role,
            role.Id,
            _currentUserService.UserId ?? Guid.Empty,
            $"Role deleted: {role.Name}",
            cancellationToken);

        return Result<bool>.SuccessResult(true);
    }
}
