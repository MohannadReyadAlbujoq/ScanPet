using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Common.Queries;

/// <summary>
/// Base query for search operations with pagination support
/// Eliminates duplication across all search queries
/// </summary>
/// <typeparam name="TDto">The DTO type to return in results</typeparam>
public abstract class BaseSearchQuery<TDto> : IRequest<Result<List<TDto>>>
{
    /// <summary>
    /// Search term to match against entity fields
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// Page number for pagination (optional)
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Page size for pagination (optional)
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Validates the search query
    /// </summary>
    public virtual bool IsValid(out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            errorMessage = "Search term is required";
            return false;
        }

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

        errorMessage = null;
        return true;
    }
}
