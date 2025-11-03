using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Queries.SearchItems;

/// <summary>
/// Handler for searching items by name, description, or SKU
/// Uses BaseSearchHandler to eliminate code duplication
/// </summary>
public class SearchItemsQueryHandler : BaseSearchHandler<SearchItemsQuery, Item, ItemDto>
{
    private readonly IItemRepository _itemRepository;

    public SearchItemsQueryHandler(
        IItemRepository itemRepository,
        ILogger<SearchItemsQueryHandler> logger)
        : base(logger)
    {
        _itemRepository = itemRepository;
    }

    protected override async Task<List<Item>> GetAllEntitiesAsync(CancellationToken cancellationToken)
    {
        // Use eager loading for better performance
        var items = await _itemRepository.GetAllWithColorsAsync(cancellationToken);
        return items.ToList();
    }

    protected override bool MatchesSearchTerm(Item entity, string searchTerm)
    {
        return entity.Name.ToLower().Contains(searchTerm) ||
               (entity.Description != null && entity.Description.ToLower().Contains(searchTerm)) ||
               (entity.SKU != null && entity.SKU.ToLower().Contains(searchTerm));
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
            ColorName = entity.Color?.Name,
            ImageUrl = entity.ImageUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Item;
}
