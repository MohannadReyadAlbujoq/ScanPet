using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Queries.SearchColors;

/// <summary>
/// Query to search colors by name or description
/// Inherits pagination and search term from BaseSearchQuery
/// </summary>
public class SearchColorsQuery : BaseSearchQuery<ColorDto>, IRequest<Result<List<ColorDto>>>
{
    // All properties inherited from BaseSearchQuery:
    // - SearchTerm
    // - PageNumber
    // - PageSize
    // - IsValid() method
}
