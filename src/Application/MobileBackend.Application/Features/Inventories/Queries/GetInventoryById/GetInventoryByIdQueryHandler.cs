using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Queries.GetInventoryById;

/// <summary>
/// Handler for getting inventory by ID
/// ? Optimized - Uses SQL aggregation (No N+1 queries)
/// ? Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetInventoryByIdQueryHandler : BaseGetByIdHandler<GetInventoryByIdQuery, Inventory, InventoryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private int _itemCount;

    public GetInventoryByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetInventoryByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Inventory?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // ? Use optimized method with SQL aggregation - single query!
        var (inventory, itemCount) = await _unitOfWork.Inventories.GetByIdWithItemCountAsync(id, cancellationToken);
        _itemCount = itemCount;
        return inventory;
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
            TotalItems = _itemCount,  // ? Accurate count from SQL aggregation!
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Inventory;
}
