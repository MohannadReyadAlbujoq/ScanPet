using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;

namespace MobileBackend.Application.Features.Locations.Queries.GetAllLocations;

/// <summary>
/// Query to get all locations with optional pagination
/// </summary>
public class GetAllLocationsQuery : BasePagedQuery<LocationDto>
{
}
