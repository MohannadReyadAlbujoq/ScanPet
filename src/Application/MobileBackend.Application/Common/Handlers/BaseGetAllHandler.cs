using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Handlers;

/// <summary>
/// Base handler for GetAll operations with optional pagination
/// Implements common list retrieval logic
/// </summary>
/// <typeparam name="TQuery">The query type</typeparam>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TDto">The DTO type to return</typeparam>
public abstract class BaseGetAllHandler<TQuery, TEntity, TDto> : IRequestHandler<TQuery, Result<List<TDto>>>
    where TQuery : BasePagedQuery<TDto>
    where TEntity : BaseEntity
{
    protected readonly ILogger Logger;

    protected BaseGetAllHandler(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<Result<List<TDto>>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate query
            if (!request.IsValid(out var errorMessage))
            {
                return Result<List<TDto>>.FailureResult(errorMessage!, 400);
            }

            Logger.LogInformation("Getting all {EntityName} (Paginated: {IsPaginated})", 
                GetEntityName(), request.IsPaginated);

            // Get entities with optional pagination
            var entities = await GetEntitiesAsync(request, cancellationToken);

            // Map to DTOs
            var dtos = entities.Select(MapToDto).ToList();

            Logger.LogInformation("Retrieved {Count} {EntityName}", dtos.Count, GetEntityName());

            return Result<List<TDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving {EntityName}", GetEntityName());
            return Result<List<TDto>>.FailureResult(
                $"An error occurred while retrieving {GetEntityName()}", 500);
        }
    }

    /// <summary>
    /// Gets entities from repository with optional pagination
    /// </summary>
    protected abstract Task<List<TEntity>> GetEntitiesAsync(TQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Maps entity to DTO
    /// </summary>
    protected abstract TDto MapToDto(TEntity entity);

    /// <summary>
    /// Gets the entity name for logging
    /// </summary>
    protected abstract string GetEntityName();
}
