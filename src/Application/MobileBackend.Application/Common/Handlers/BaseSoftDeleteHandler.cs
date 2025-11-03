using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Handlers;

/// <summary>
/// Base handler for soft delete operations
/// Eliminates code duplication across all delete command handlers
/// </summary>
public abstract class BaseSoftDeleteHandler<TCommand, TEntity> : IRequestHandler<TCommand, Result<bool>>
    where TCommand : IRequest<Result<bool>>
    where TEntity : class, ISoftDelete
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IAuditService AuditService;
    protected readonly ICurrentUserService CurrentUserService;
    protected readonly IDateTimeService DateTimeService;
    protected readonly ILogger Logger;

    protected BaseSoftDeleteHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger logger)
    {
        UnitOfWork = unitOfWork;
        AuditService = auditService;
        CurrentUserService = currentUserService;
        DateTimeService = dateTimeService;
        Logger = logger;
    }

    public async Task<Result<bool>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Extract entity ID from command
            var entityId = GetEntityId(request);

            // 2. Get entity from repository
            var entity = await GetEntityAsync(entityId, cancellationToken);
            if (entity == null)
            {
                return Result<bool>.FailureResult(ErrorMessages.NotFound(GetEntityName()), 404);
            }

            // 3. Validate deletion (optional - override if needed)
            var validationResult = await ValidateDeletionAsync(entity, cancellationToken);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            // 4. Perform soft delete
            entity.IsDeleted = true;
            entity.DeletedAt = DateTimeService.UtcNow;
            entity.DeletedBy = CurrentUserService.UserId;

            // 5. Update entity
            UpdateEntity(entity);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Audit log
            await AuditService.LogAsync(
                action: GetAuditAction(),
                entityName: GetEntityName(),
                entityId: entityId,
                userId: CurrentUserService.UserId ?? Guid.Empty,
                additionalInfo: GetAuditMessage(entity),
                cancellationToken: cancellationToken
            );

            Logger.LogInformation("{EntityName} deleted successfully: {EntityId}", 
                GetEntityName(), entityId);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {EntityName}: {EntityId}", 
                GetEntityName(), GetEntityId(request));
            return Result<bool>.FailureResult(ErrorMessages.DeleteFailed(GetEntityName()), 500);
        }
    }

    // Abstract methods - must be implemented by derived classes
    protected abstract Guid GetEntityId(TCommand command);
    protected abstract Task<TEntity?> GetEntityAsync(Guid id, CancellationToken cancellationToken);
    protected abstract void UpdateEntity(TEntity entity);
    protected abstract string GetEntityName();
    protected abstract string GetAuditAction();
    protected abstract string GetAuditMessage(TEntity entity);

    // Virtual method - can be overridden if validation is needed
    protected virtual Task<Result<bool>> ValidateDeletionAsync(
        TEntity entity, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<bool>.SuccessResult(true));
    }
}
