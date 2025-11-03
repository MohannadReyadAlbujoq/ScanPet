using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Queries.GetAllItems;

/// <summary>
/// Handler for getting all items (with colors included - no N+1)
/// Uses BaseGetAllHandler to eliminate code duplication
/// </summary>
public class GetAllItemsQueryHandler : BaseGetAllHandler<GetAllItemsQuery, Item, ItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllItemsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllItemsQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<List<Item>> GetEntitiesAsync(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        // Use optimized method that includes colors ?
        var items = await _unitOfWork.Items.GetAllWithColorsAsync(cancellationToken);
        return items.ToList();
    }

    protected override ItemDto MapToDto(Item entity)
    {
        return new ItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SKU = entity.SKU,
            BasePrice = entity.BasePrice,
            Quantity = entity.Quantity,
            ColorId = entity.ColorId,
            ColorName = entity.Color?.Name,  // ? Now works correctly!
            ImageUrl = entity.ImageUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Item;
}
