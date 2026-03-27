using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Queries.GetItemById;

/// <summary>
/// Handler for getting an item by ID (with color included)
/// Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetItemByIdQueryHandler : BaseGetByIdHandler<GetItemByIdQuery, Item, ItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetItemByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetItemByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Item?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // Use optimized method that includes color ?
        return await _unitOfWork.Items.GetByIdWithColorAsync(id, cancellationToken);
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
            ColorId = entity.ColorId,
            ColorName = entity.Color?.Name,  // ? Now works correctly!
            ImageUrl = entity.ImageUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Item;
}
