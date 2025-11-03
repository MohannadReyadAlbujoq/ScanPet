using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Queries.SearchLocations;

/// <summary>
/// Handler for searching locations by name, city, or country
/// Uses BaseSearchHandler to eliminate code duplication
/// </summary>
public class SearchLocationsQueryHandler : BaseSearchHandler<SearchLocationsQuery, Location, LocationDto>
{
    private readonly ILocationRepository _locationRepository;

    public SearchLocationsQueryHandler(
        ILocationRepository locationRepository,
        ILogger<SearchLocationsQueryHandler> logger)
        : base(logger)
    {
        _locationRepository = locationRepository;
    }

    protected override async Task<List<Location>> GetAllEntitiesAsync(CancellationToken cancellationToken)
    {
        var locations = await _locationRepository.GetAllAsync(cancellationToken);
        return locations.ToList();
    }

    protected override bool MatchesSearchTerm(Location entity, string searchTerm)
    {
        return entity.Name.ToLower().Contains(searchTerm) ||
               (entity.City != null && entity.City.ToLower().Contains(searchTerm)) ||
               (entity.Country != null && entity.Country.ToLower().Contains(searchTerm)) ||
               (entity.Address != null && entity.Address.ToLower().Contains(searchTerm));
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
            OrderCount = entity.Orders?.Count(o => !o.IsDeleted) ?? 0,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Location;
}
