using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Items.Queries.GetItemById;

/// <summary>
/// Handler for getting an item by ID
/// </summary>
public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, Result<ItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetItemByIdQueryHandler> _logger;

    public GetItemByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetItemByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ItemDto>> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId);

            if (item == null)
            {
                return Result<ItemDto>.FailureResult("Item not found", 404);
            }

            var itemDto = new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                SKU = item.SKU,
                BasePrice = item.BasePrice,
                Quantity = item.Quantity,
                ColorId = item.ColorId,
                ColorName = item.Color?.Name,
                ImageUrl = item.ImageUrl,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };

            _logger.LogInformation("Retrieved item: {ItemId} - {ItemName}", item.Id, item.Name);

            return Result<ItemDto>.SuccessResult(itemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item: {ItemId}", request.ItemId);
            return Result<ItemDto>.FailureResult("An error occurred while retrieving the item", 500);
        }
    }
}
