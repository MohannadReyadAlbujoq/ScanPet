using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Common.Queries;

/// <summary>
/// Base query for paginated list operations
/// Eliminates duplication across all GetAll queries with pagination
/// </summary>
/// <typeparam name="TDto">The DTO type to return in results</typeparam>
public abstract class BasePagedQuery<TDto> : IRequest<Result<List<TDto>>>
{
    /// <summary>
    /// Page number (optional, returns all if not specified)
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Page size (optional, returns all if not specified)
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Whether pagination is requested
    /// </summary>
    public bool IsPaginated => PageNumber.HasValue && PageSize.HasValue;

    /// <summary>
    /// Validates the paged query
    /// </summary>
    public virtual bool IsValid(out string? errorMessage)
    {
        if (PageNumber.HasValue && PageNumber.Value < 1)
        {
            errorMessage = "Page number must be greater than 0";
            return false;
        }

        if (PageSize.HasValue && (PageSize.Value < 1 || PageSize.Value > 100))
        {
            errorMessage = "Page size must be between 1 and 100";
            return false;
        }

        // Both must be specified or both must be null
        if ((PageNumber.HasValue && !PageSize.HasValue) || (!PageNumber.HasValue && PageSize.HasValue))
        {
            errorMessage = "Both PageNumber and PageSize must be specified together";
            return false;
        }

        errorMessage = null;
        return true;
    }

    /// <summary>
    /// Gets the skip count for pagination
    /// </summary>
    public int GetSkip() => (PageNumber!.Value - 1) * PageSize!.Value;

    /// <summary>
    /// Gets the take count for pagination
    /// </summary>
    public int GetTake() => PageSize!.Value;
}
