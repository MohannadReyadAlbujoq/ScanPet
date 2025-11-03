using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Handlers;

/// <summary>
/// Base handler for search operations
/// Implements common search logic with pagination
/// </summary>
/// <typeparam name="TQuery">The search query type</typeparam>
/// <typeparam name="TEntity">The entity type being searched</typeparam>
/// <typeparam name="TDto">The DTO type to return</typeparam>
public abstract class BaseSearchHandler<TQuery, TEntity, TDto> : IRequestHandler<TQuery, Result<List<TDto>>>
    where TQuery : BaseSearchQuery<TDto>
    where TEntity : BaseEntity
{
    protected readonly ILogger Logger;

    protected BaseSearchHandler(ILogger logger)
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

            Logger.LogInformation("Searching {EntityName} with term: {SearchTerm}", 
                GetEntityName(), request.SearchTerm);

            // Get all entities
            var entities = await GetAllEntitiesAsync(cancellationToken);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                entities = entities.Where(e => MatchesSearchTerm(e, searchTerm)).ToList();
            }

            // Apply pagination if specified
            if (request.PageNumber.HasValue && request.PageSize.HasValue)
            {
                var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
                entities = entities.Skip(skip).Take(request.PageSize.Value).ToList();
            }

            // Map to DTOs
            var dtos = entities.Select(MapToDto).ToList();

            Logger.LogInformation("Found {Count} {EntityName} matching search term", 
                dtos.Count, GetEntityName());

            return Result<List<TDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching {EntityName} with term: {SearchTerm}", 
                GetEntityName(), request.SearchTerm);
            return Result<List<TDto>>.FailureResult(
                $"An error occurred while searching {GetEntityName()}", 500);
        }
    }

    /// <summary>
    /// Gets all entities from the repository
    /// </summary>
    protected abstract Task<List<TEntity>> GetAllEntitiesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks if an entity matches the search term
    /// </summary>
    protected abstract bool MatchesSearchTerm(TEntity entity, string searchTerm);

    /// <summary>
    /// Maps entity to DTO
    /// </summary>
    protected abstract TDto MapToDto(TEntity entity);

    /// <summary>
    /// Gets the entity name for logging
    /// </summary>
    protected abstract string GetEntityName();
}
