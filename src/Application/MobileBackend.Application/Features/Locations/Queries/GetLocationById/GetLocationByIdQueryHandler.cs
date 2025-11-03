using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Locations;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Queries.GetLocationById;

/// <summary>
/// Handler for getting a location by ID (with accurate order count)
/// Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetLocationByIdQueryHandler : BaseGetByIdHandler<GetLocationByIdQuery, Location, LocationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private int _orderCount = 0;

    public GetLocationByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetLocationByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Location?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // Use optimized method with SQL aggregation ?
        var (location, orderCount) = await _unitOfWork.Locations.GetByIdWithOrderCountAsync(id, cancellationToken);
        
        // Store order count for use in MapToDto
        _orderCount = orderCount;
        
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
            OrderCount = _orderCount,  // ? Accurate count from SQL!
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Location;
}
