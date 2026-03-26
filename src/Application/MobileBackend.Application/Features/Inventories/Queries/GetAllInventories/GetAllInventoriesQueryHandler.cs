using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Queries.GetAllInventories;

/// <summary>
/// Handler for getting all inventories/warehouses
/// ? Optimized - Uses SQL aggregation (No N+1 queries)
/// ? Uses BaseGetAllHandler to eliminate code duplication
/// </summary>
public class GetAllInventoriesQueryHandler : BaseGetAllHandler<GetAllInventoriesQuery, Inventory, InventoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private Dictionary<Guid, int> _itemCounts = new();

    public GetAllInventoriesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllInventoriesQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<List<Inventory>> GetEntitiesAsync(GetAllInventoriesQuery request, CancellationToken cancellationToken)
    {
        // ? Use optimized method with SQL aggregation - single query!
        var inventoriesWithCounts = await _unitOfWork.Inventories.GetAllWithItemCountsAsync(cancellationToken);
        
        // Store counts for use in MapToDto
        _itemCounts = inventoriesWithCounts.ToDictionary(x => x.Inventory.Id, x => x.ItemCount);
        
        return inventoriesWithCounts.Select(x => x.Inventory).ToList();
    }

    protected override InventoryDto MapToDto(Inventory entity)
    {
        return new InventoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Location = entity.Location,
            Description = entity.Description,
            IsActive = entity.IsActive,
            TotalItems = _itemCounts.GetValueOrDefault(entity.Id),  // ? Use cached count
            LocationId = entity.LocationId,
            LocationName = entity.ParentLocation?.Name,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Inventory;
}
