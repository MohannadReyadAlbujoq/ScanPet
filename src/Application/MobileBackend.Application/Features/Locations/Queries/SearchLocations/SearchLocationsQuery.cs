using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Locations;

namespace MobileBackend.Application.Features.Locations.Queries.SearchLocations;

/// <summary>
/// Query to search locations by name, city, or country
/// Inherits pagination and search term from BaseSearchQuery
/// </summary>
public class SearchLocationsQuery : BaseSearchQuery<LocationDto>
{
    // All properties inherited from BaseSearchQuery:
    // - SearchTerm
    // - PageNumber
    // - PageSize
    // - IsValid() method
}
