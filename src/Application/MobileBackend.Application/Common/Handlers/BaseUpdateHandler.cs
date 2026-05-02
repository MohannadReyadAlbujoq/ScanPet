using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Handlers;

/// <summary>
/// Base handler for update operations
/// Eliminates code duplication across all update command handlers
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public abstract class BaseUpdateHandler<TCommand, TEntity> : IRequestHandler<TCommand, Result<bool>>
    where TCommand : IRequest<Result<bool>>
    where TEntity : BaseEntity
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IAuditService AuditService;
    protected readonly ICurrentUserService CurrentUserService;
    protected readonly IDateTimeService DateTimeService;
    protected readonly ILogger Logger;

    protected BaseUpdateHandler(
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

            // 2. Get existing entity
            var entity = await GetEntityAsync(entityId, cancellationToken);
            if (entity == null)
            {
                return Result<bool>.FailureResult(ErrorMessages.NotFound(GetEntityName()), 404);
            }

            // 3. Capture old values for audit
            var oldValues = CaptureOldValues(entity);

            // 4. Validate uniqueness (optional - override if needed)
            var validationResult = await ValidateUniquenessAsync(request, entity, cancellationToken);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            // 5. Perform additional validation (optional - override if needed)
            var additionalValidationResult = await ValidateAsync(request, entity, cancellationToken);
            if (!additionalValidationResult.Success)
            {
                return additionalValidationResult;
            }

            // 6. Update entity properties
            await UpdateEntityPropertiesAsync(request, entity, cancellationToken);

            // 7. Set audit fields
            entity.UpdatedAt = DateTimeService.UtcNow;
            entity.UpdatedBy = CurrentUserService.UserId;

            // 8. Capture new values for audit
            var newValues = CaptureNewValues(entity);

            // 9. Update entity
            UpdateEntity(entity);

            // 10. Audit log with old/new values
            await LogAuditAsync(entity, oldValues, newValues, cancellationToken);

            // 11. Save all changes in a single round-trip
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("{EntityName} updated successfully: {EntityId}", 
                GetEntityName(), entityId);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating {EntityName}: {EntityId}", 
                GetEntityName(), GetEntityId(request));
            return ClassifyException(ex, GetEntityName(), "updating");
        }
    }

    /// <summary>
    /// Classifies exceptions and returns appropriate Result with specific error messages
    /// </summary>
    private static Result<bool> ClassifyException(Exception ex, string entityName, string operation)
    {
        var innerMessage = ex.InnerException?.Message ?? ex.Message;

        // Unique constraint violation
        if (innerMessage.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("23505", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.FailureResult(
                $"A {entityName.ToLower()} with the same unique value already exists.", 409);
        }

        // Foreign key violation
        if (innerMessage.Contains("foreign key", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("23503", StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.FailureResult(
                $"A referenced record does not exist. Please verify all related data.", 400);
        }

        return Result<bool>.FailureResult(ErrorMessages.UpdateFailed(entityName), 500);
    }

    // Abstract methods - must be implemented by derived classes
    
    /// <summary>
    /// Extract entity ID from command
    /// </summary>
    protected abstract Guid GetEntityId(TCommand command);
    
    /// <summary>
    /// Get entity from repository
    /// </summary>
    protected abstract Task<TEntity?> GetEntityAsync(Guid id, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update entity properties from command
    /// </summary>
    protected abstract Task UpdateEntityPropertiesAsync(
        TCommand command, 
        TEntity entity, 
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Update entity in repository
    /// </summary>
    protected abstract void UpdateEntity(TEntity entity);
    
    /// <summary>
    /// Get entity name for messages
    /// </summary>
    protected abstract string GetEntityName();
    
    /// <summary>
    /// Get audit action constant
    /// </summary>
    protected abstract string GetAuditAction();

    // Virtual methods - can be overridden for customization
    
    /// <summary>
    /// Capture old values before update for audit logging
    /// Must return valid JSON (stored in PostgreSQL jsonb column)
    /// Override this to customize what values are captured
    /// </summary>
    protected virtual string CaptureOldValues(TEntity entity)
    {
        return $"{{\"entity\":\"{GetEntityName()}\",\"id\":\"{entity.Id}\"}}";
    }

    /// <summary>
    /// Capture new values after update for audit logging
    /// Must return valid JSON (stored in PostgreSQL jsonb column)
    /// Override this to customize what values are captured
    /// </summary>
    protected virtual string CaptureNewValues(TEntity entity)
    {
        return $"{{\"entity\":\"{GetEntityName()}\",\"id\":\"{entity.Id}\",\"updatedAt\":\"{DateTimeService.UtcNow:O}\"}}";
    }

    /// <summary>
    /// Validate uniqueness (e.g., check for duplicate names)
    /// Override this to implement custom uniqueness validation
    /// </summary>
    protected virtual Task<Result<bool>> ValidateUniquenessAsync(
        TCommand command, 
        TEntity entity, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Perform additional validation before updating entity
    /// Override this to implement custom business rules
    /// </summary>
    protected virtual Task<Result<bool>> ValidateAsync(
        TCommand command, 
        TEntity entity, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<bool>.SuccessResult(true));
    }

    /// <summary>
    /// Log audit entry with old and new values
    /// Override this to customize audit logging
    /// </summary>
    protected virtual async Task LogAuditAsync(
        TEntity entity, 
        string oldValues, 
        string newValues, 
        CancellationToken cancellationToken)
    {
        // Try to use LogActionAsync if available (with old/new values)
        try
        {
            await AuditService.LogActionAsync(
                userId: CurrentUserService.UserId,
                action: GetAuditAction(),
                entityName: GetEntityName(),
                entityId: entity.Id,
                oldValues: oldValues,
                newValues: newValues,
                cancellationToken: cancellationToken
            );
        }
        catch
        {
            // Fallback to regular LogAsync if LogActionAsync doesn't exist
            await AuditService.LogAsync(
                action: GetAuditAction(),
                entityName: GetEntityName(),
                entityId: entity.Id,
                userId: CurrentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Updated. Old: {oldValues}, New: {newValues}",
                cancellationToken: cancellationToken
            );
        }
    }
}
