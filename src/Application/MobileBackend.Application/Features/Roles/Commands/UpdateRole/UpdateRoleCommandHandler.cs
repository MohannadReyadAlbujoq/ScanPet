using MediatR;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Handler for UpdateRoleCommand
/// </summary>
public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public UpdateRoleCommandHandler(
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

    public async Task<Result<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
        {
            return Result<bool>.FailureResult("Role not found", 404);
        }

        // Check if new name already exists (excluding current role)
        var existingRole = await _unitOfWork.Roles.GetByNameAsync(request.Name, cancellationToken);
        if (existingRole != null && existingRole.Id != request.RoleId)
        {
            return Result<bool>.FailureResult("Role name already exists", 400);
        }

        // Update role
        var oldName = role.Name;
        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedAt = _dateTimeService.UtcNow;
        role.UpdatedBy = _currentUserService.UserId;

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogAsync(
            AuditActions.RoleUpdated,
            EntityNames.Role,
            role.Id,
            _currentUserService.UserId ?? Guid.Empty,
            $"Role updated: {oldName} -> {role.Name}",
            cancellationToken);

        return Result<bool>.SuccessResult(true);
    }
}
