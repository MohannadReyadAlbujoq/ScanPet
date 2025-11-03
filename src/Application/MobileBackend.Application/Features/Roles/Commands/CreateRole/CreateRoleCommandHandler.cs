using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Roles.Commands.CreateRole;

/// <summary>
/// Handler for CreateRoleCommand
/// Uses BaseCreateHandler to eliminate code duplication
/// </summary>
public class CreateRoleCommandHandler : BaseCreateHandler<CreateRoleCommand, Role>
{
    public CreateRoleCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateRoleCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override async Task<Role> CreateEntityAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            IsDeleted = false
        };
    }

    protected override Task AddEntityAsync(Role entity, CancellationToken cancellationToken)
    {
        return UnitOfWork.Roles.AddAsync(entity, cancellationToken);
    }

    protected override string GetEntityName() => EntityNames.Role;

    protected override string GetAuditAction() => AuditActions.RoleCreated;

    protected override string GetAuditMessage(Role entity)
        => $"Role created: {entity.Name}";

    // Override uniqueness validation
    protected override async Task<Result<Guid>> ValidateUniquenessAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var existingRole = await UnitOfWork.Roles.GetByNameAsync(command.Name, cancellationToken);
        if (existingRole != null)
        {
            return Result<Guid>.FailureResult(ErrorMessages.AlreadyExists("Role", "name"), 409);
        }
        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
