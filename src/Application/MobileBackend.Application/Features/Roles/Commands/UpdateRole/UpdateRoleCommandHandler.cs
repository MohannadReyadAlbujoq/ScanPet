using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Roles.Commands.UpdateRole;

/// <summary>
/// Handler for UpdateRoleCommand
/// Uses BaseUpdateHandler to eliminate code duplication
/// </summary>
public class UpdateRoleCommandHandler : BaseUpdateHandler<UpdateRoleCommand, Role>
{
    public UpdateRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<UpdateRoleCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(UpdateRoleCommand command) => command.RoleId;

    protected override Task<Role?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => UnitOfWork.Roles.GetByIdAsync(id, cancellationToken);

    protected override async Task UpdateEntityPropertiesAsync(
        UpdateRoleCommand command,
        Role entity,
        CancellationToken cancellationToken)
    {
        entity.Name = command.Name;
        entity.Description = command.Description;
    }

    protected override void UpdateEntity(Role entity)
    {
        UnitOfWork.Roles.Update(entity);
    }

    protected override string GetEntityName() => EntityNames.Role;

    protected override string GetAuditAction() => AuditActions.RoleUpdated;

    protected override string CaptureOldValues(Role entity)
        => $"Name: {entity.Name}";

    protected override string CaptureNewValues(Role entity)
        => $"Name: {entity.Name}";

    // Override uniqueness validation
    protected override async Task<Result<bool>> ValidateUniquenessAsync(
        UpdateRoleCommand command,
        Role entity,
        CancellationToken cancellationToken)
    {
        var existingRole = await UnitOfWork.Roles.GetByNameAsync(command.Name, cancellationToken);
        if (existingRole != null && existingRole.Id != command.RoleId)
        {
            return Result<bool>.FailureResult(ErrorMessages.AlreadyExists("Role", "name"), 409);
        }
        return Result<bool>.SuccessResult(true);
    }
}
