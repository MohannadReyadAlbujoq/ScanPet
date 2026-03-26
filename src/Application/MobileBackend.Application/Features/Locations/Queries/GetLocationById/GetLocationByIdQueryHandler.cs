using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Queries.GetLocationById;

/// <summary>
/// Handler for getting a location by ID (with sections/inventories detail)
/// Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetLocationByIdQueryHandler : BaseGetByIdHandler<GetLocationByIdQuery, Location, LocationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private int _orderCount = 0;
    private List<Inventory> _sections = new();

    public GetLocationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetLocationByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Location?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var (location, orderCount, sections) = await _unitOfWork.Locations.GetByIdWithDetailsAsync(id, cancellationToken);
        
        _orderCount = orderCount;
        _sections = sections;
        
        return location;
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
            OrderCount = _orderCount,
            SectionCount = _sections.Count,
            Sections = _sections.Select(s => new InventoryDto
            {
                Id = s.Id,
                Name = s.Name,
                Location = s.Location,
                Description = s.Description,
                IsActive = s.IsActive,
                LocationId = s.LocationId,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Location;
}
