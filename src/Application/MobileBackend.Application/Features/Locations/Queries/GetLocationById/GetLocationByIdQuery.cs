using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;

namespace MobileBackend.Application.Features.Locations.Queries.GetLocationById;

/// <summary>
/// Query to get a location by ID
/// </summary>
public class GetLocationByIdQuery : BaseGetByIdQuery<LocationDto>, IRequest<Result<LocationDto>>
{
    // Backwards compatibility: Allow LocationId property
    public Guid LocationId 
    { 
        get => Id; 
        set => Id = value; 
    }
}
