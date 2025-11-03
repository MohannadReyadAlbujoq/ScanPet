using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Locations.Queries.GetAllLocations;

/// <summary>
/// Handler for getting all locations (with accurate order counts - no N+1)
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
            // Use optimized method with SQL aggregation ?
            var locationsWithCounts = await _unitOfWork.Locations.GetAllWithOrderCountsAsync(cancellationToken);

            var locationDtos = locationsWithCounts.Select(tuple => new LocationDto
            {
                Id = tuple.Location.Id,
                Name = tuple.Location.Name,
                Address = tuple.Location.Address,
                City = tuple.Location.City,
                Country = tuple.Location.Country,
                PostalCode = tuple.Location.PostalCode,
                IsActive = tuple.Location.IsActive,
                OrderCount = tuple.OrderCount,  // ? Accurate count!
                CreatedAt = tuple.Location.CreatedAt,
                UpdatedAt = tuple.Location.UpdatedAt
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
