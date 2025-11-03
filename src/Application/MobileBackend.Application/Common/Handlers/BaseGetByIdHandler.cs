using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Handlers;

/// <summary>
/// Base handler for GetById operations
/// Implements common single entity retrieval logic
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDto">The DTO type to return</typeparam>
public abstract class BaseGetByIdHandler<TQuery, TEntity, TDto> : IRequestHandler<TQuery, Result<TDto>>
    where TQuery : BaseGetByIdQuery<TDto>
    where TEntity : BaseEntity
{
    protected readonly ILogger Logger;

    protected BaseGetByIdHandler(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<Result<TDto>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate query
            if (!request.IsValid(out var errorMessage))
            {
                return Result<TDto>.FailureResult(errorMessage!, 400);
            }

            Logger.LogInformation("Getting {EntityName} by ID: {EntityId}", 
                GetEntityName(), request.Id);

            // Get entity
            var entity = await GetEntityByIdAsync(request.Id, cancellationToken);

            if (entity == null)
            {
                Logger.LogWarning("{EntityName} not found: {EntityId}", GetEntityName(), request.Id);
                return Result<TDto>.FailureResult(ErrorMessages.NotFound(GetEntityName()), 404);
            }

            // Map to DTO
            var dto = MapToDto(entity);

            Logger.LogInformation("{EntityName} retrieved successfully: {EntityId}", 
                GetEntityName(), request.Id);

            return Result<TDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving {EntityName} by ID: {EntityId}", 
                GetEntityName(), request.Id);
            return Result<TDto>.FailureResult(
                $"An error occurred while retrieving {GetEntityName()}", 500);
        }
    }

    /// <summary>
    /// Gets entity by ID from repository
    /// </summary>
    protected abstract Task<TEntity?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Maps entity to DTO
    /// </summary>
    protected abstract TDto MapToDto(TEntity entity);

    /// <summary>
    /// Gets the entity name for logging
    /// </summary>
    protected abstract string GetEntityName();
}
