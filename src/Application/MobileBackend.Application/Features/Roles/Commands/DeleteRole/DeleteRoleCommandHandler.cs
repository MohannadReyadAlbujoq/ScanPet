using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Roles.Commands.DeleteRole;

/// <summary>
/// Handler for deleting (soft delete) a role
/// Uses BaseSoftDeleteHandler with validation override
/// </summary>
public class DeleteRoleCommandHandler : BaseSoftDeleteHandler<DeleteRoleCommand, Role>
{
    public DeleteRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<DeleteRoleCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(DeleteRoleCommand command) 
        => command.RoleId;

    protected override async Task<Role?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => await UnitOfWork.Roles.GetByIdAsync(id, cancellationToken);

    protected override void UpdateEntity(Role entity)
    {
        // Use the repository's soft delete method
        entity.IsDeleted = true;
        entity.DeletedAt = DateTimeService.UtcNow;
        entity.DeletedBy = CurrentUserService.UserId;
        UnitOfWork.Roles.Update(entity);
    }

    protected override string GetEntityName() 
        => EntityNames.Role;

    protected override string GetAuditAction() 
        => AuditActions.RoleDeleted;

    protected override string GetAuditMessage(Role entity)
        => $"Deleted role: {entity.Name}";

    // Override validation to check if role is assigned to users
    protected override async Task<Result<bool>> ValidateDeletionAsync(
        Role entity, 
        CancellationToken cancellationToken)
    {
        var usersWithRole = await UnitOfWork.Roles.GetUserCountAsync(entity.Id, cancellationToken);
        if (usersWithRole > 0)
        {
            return Result<bool>.FailureResult(ErrorMessages.RoleInUse(usersWithRole), 400);
        }

        return Result<bool>.SuccessResult(true);
    }
}
