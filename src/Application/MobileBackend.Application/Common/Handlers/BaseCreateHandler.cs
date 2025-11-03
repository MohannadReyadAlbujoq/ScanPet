using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Handlers;

/// <summary>
/// Base handler for create operations
/// Eliminates code duplication across all create command handlers
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public abstract class BaseCreateHandler<TCommand, TEntity> : IRequestHandler<TCommand, Result<Guid>>
    where TCommand : IRequest<Result<Guid>>
    where TEntity : BaseEntity
{
    protected readonly IUnitOfWork UnitOfWork;
    protected readonly IAuditService AuditService;
    protected readonly ICurrentUserService CurrentUserService;
    protected readonly IDateTimeService DateTimeService;
    protected readonly ILogger Logger;

    protected BaseCreateHandler(
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

    public async Task<Result<Guid>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Validate uniqueness (optional - override if needed)
            var validationResult = await ValidateUniquenessAsync(request, cancellationToken);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            // 2. Perform additional validation (optional - override if needed)
            var additionalValidationResult = await ValidateAsync(request, cancellationToken);
            if (!additionalValidationResult.Success)
            {
                return additionalValidationResult;
            }

            // 3. Create entity from command
            var entity = await CreateEntityAsync(request, cancellationToken);

            // 4. Set audit fields
            entity.CreatedAt = DateTimeService.UtcNow;
            entity.CreatedBy = CurrentUserService.UserId;

            // 5. Add to repository
            await AddEntityAsync(entity, cancellationToken);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Audit log
            await AuditService.LogAsync(
                action: GetAuditAction(),
                entityName: GetEntityName(),
                entityId: entity.Id,
                userId: CurrentUserService.UserId ?? Guid.Empty,
                additionalInfo: GetAuditMessage(entity),
                cancellationToken: cancellationToken
            );

            Logger.LogInformation("{EntityName} created successfully: {EntityId}", 
                GetEntityName(), entity.Id);

            return Result<Guid>.SuccessResult(entity.Id, 201);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating {EntityName}", GetEntityName());
            return Result<Guid>.FailureResult(ErrorMessages.CreateFailed(GetEntityName()), 500);
        }
    }

    // Abstract methods - must be implemented by derived classes
    
    /// <summary>
    /// Create entity instance from command
    /// </summary>
    protected abstract Task<TEntity> CreateEntityAsync(TCommand command, CancellationToken cancellationToken);
    
    /// <summary>
    /// Add entity to repository
    /// </summary>
    protected abstract Task AddEntityAsync(TEntity entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get entity name for messages
    /// </summary>
    protected abstract string GetEntityName();
    
    /// <summary>
    /// Get audit action constant
    /// </summary>
    protected abstract string GetAuditAction();
    
    /// <summary>
    /// Get audit message
    /// </summary>
    protected abstract string GetAuditMessage(TEntity entity);

    // Virtual methods - can be overridden if validation is needed
    
    /// <summary>
    /// Validate uniqueness (e.g., check for duplicate names)
    /// Override this to implement custom uniqueness validation
    /// </summary>
    protected virtual Task<Result<Guid>> ValidateUniquenessAsync(
        TCommand command, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Guid>.SuccessResult(Guid.Empty));
    }

    /// <summary>
    /// Perform additional validation before creating entity
    /// Override this to implement custom business rules
    /// </summary>
    protected virtual Task<Result<Guid>> ValidateAsync(
        TCommand command, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<Guid>.SuccessResult(Guid.Empty));
    }
}
