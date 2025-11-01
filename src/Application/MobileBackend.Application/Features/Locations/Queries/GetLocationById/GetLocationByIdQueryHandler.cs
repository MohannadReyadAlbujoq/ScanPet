using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Locations.Queries.GetLocationById;

/// <summary>
/// Handler for getting a location by ID
/// </summary>
public class GetLocationByIdQueryHandler : IRequestHandler<GetLocationByIdQuery, Result<LocationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetLocationByIdQueryHandler> _logger;

    public GetLocationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetLocationByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LocationDto>> Handle(GetLocationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);

            if (location == null)
            {
                return Result<LocationDto>.FailureResult("Location not found", 404);
            }

            var locationDto = new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                City = location.City,
                Country = location.Country,
                PostalCode = location.PostalCode,
                IsActive = location.IsActive,
                OrderCount = location.Orders?.Count ?? 0,
                CreatedAt = location.CreatedAt,
                UpdatedAt = location.UpdatedAt
            };

            _logger.LogInformation("Retrieved location: {LocationId} - {LocationName}", location.Id, location.Name);

            return Result<LocationDto>.SuccessResult(locationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving location: {LocationId}", request.LocationId);
            return Result<LocationDto>.FailureResult("An error occurred while retrieving the location", 500);
        }
    }
}
