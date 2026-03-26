using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Queries.GetAllLocations;

/// <summary>
/// Handler for getting all locations (with accurate order counts and section counts)
/// Uses BaseGetAllHandler to eliminate code duplication
/// </summary>
public class GetAllLocationsQueryHandler : BaseGetAllHandler<GetAllLocationsQuery, Location, LocationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private Dictionary<Guid, int> _orderCounts = new();
    private Dictionary<Guid, int> _sectionCounts = new();

    public GetAllLocationsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllLocationsQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<List<Location>> GetEntitiesAsync(GetAllLocationsQuery request, CancellationToken cancellationToken)
    {
        var locationsWithCounts = await _unitOfWork.Locations.GetAllWithCountsAsync(cancellationToken);
        
        _orderCounts = locationsWithCounts.ToDictionary(t => t.Location.Id, t => t.OrderCount);
        _sectionCounts = locationsWithCounts.ToDictionary(t => t.Location.Id, t => t.SectionCount);
        
        return locationsWithCounts.Select(t => t.Location).ToList();
    }

    protected override LocationDto MapToDto(Location entity)
    {
        return new LocationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            City = entity.City,
            Country = entity.Country,
            PostalCode = entity.PostalCode,
            IsActive = entity.IsActive,
            OrderCount = _orderCounts.GetValueOrDefault(entity.Id),
            SectionCount = _sectionCounts.GetValueOrDefault(entity.Id),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Location;
}
