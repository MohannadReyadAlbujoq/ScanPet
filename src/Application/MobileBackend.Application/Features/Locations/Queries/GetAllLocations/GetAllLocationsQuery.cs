using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;

namespace MobileBackend.Application.Features.Locations.Queries.GetAllLocations;

/// <summary>
/// Query to get all locations
/// </summary>
public class GetAllLocationsQuery : IRequest<Result<List<LocationDto>>>
{
}
