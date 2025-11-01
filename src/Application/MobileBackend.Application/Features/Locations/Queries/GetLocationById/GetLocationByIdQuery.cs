using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;

namespace MobileBackend.Application.Features.Locations.Queries.GetLocationById;

/// <summary>
/// Query to get a location by ID
/// </summary>
public class GetLocationByIdQuery : IRequest<Result<LocationDto>>
{
    public Guid LocationId { get; set; }
}
