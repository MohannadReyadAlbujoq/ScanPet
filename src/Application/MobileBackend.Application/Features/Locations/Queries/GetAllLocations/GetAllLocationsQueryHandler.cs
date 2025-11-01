using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Locations.Queries.GetAllLocations;

/// <summary>
/// Handler for getting all locations
/// </summary>
public class GetAllLocationsQueryHandler : IRequestHandler<GetAllLocationsQuery, Result<List<LocationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllLocationsQueryHandler> _logger;

    public GetAllLocationsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllLocationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<LocationDto>>> Handle(GetAllLocationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var locations = await _unitOfWork.Locations.GetAllAsync();

            var locationDtos = locations.Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Address = l.Address,
                City = l.City,
                Country = l.Country,
                PostalCode = l.PostalCode,
                IsActive = l.IsActive,
                OrderCount = l.Orders?.Count ?? 0,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} locations", locationDtos.Count);

            return Result<List<LocationDto>>.SuccessResult(locationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations");
            return Result<List<LocationDto>>.FailureResult("An error occurred while retrieving locations", 500);
        }
    }
}
