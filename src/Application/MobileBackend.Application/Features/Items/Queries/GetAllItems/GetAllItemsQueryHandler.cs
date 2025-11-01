using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Items.Queries.GetAllItems;

/// <summary>
/// Handler for getting all items
/// </summary>
public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, Result<List<ItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllItemsQueryHandler> _logger;

    public GetAllItemsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllItemsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<ItemDto>>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _unitOfWork.Items.GetAllAsync();

            var itemDtos = items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                SKU = i.SKU,
                BasePrice = i.BasePrice,
                Quantity = i.Quantity,
                ColorId = i.ColorId,
                ColorName = i.Color?.Name,
                ImageUrl = i.ImageUrl,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} items", itemDtos.Count);

            return Result<List<ItemDto>>.SuccessResult(itemDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items");
            return Result<List<ItemDto>>.FailureResult("An error occurred while retrieving items", 500);
        }
    }
}
