using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Items;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Queries.GetAllItems;

/// <summary>
/// Handler for getting all items with DB-level pagination.
/// Uses GetPagedWithColorsAsync for efficient SQL pagination.
/// </summary>
public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, Result<PagedResult<ItemDto>>>
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

    public async Task<Result<PagedResult<ItemDto>>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate pagination parameters
            if (request.PageNumber < 1)
                return Result<PagedResult<ItemDto>>.FailureResult("Page number must be greater than 0", 400);

            if (request.PageSize < 1 || request.PageSize > 100)
                return Result<PagedResult<ItemDto>>.FailureResult("Page size must be between 1 and 100", 400);

            _logger.LogInformation("Getting items - Page: {Page}, Size: {Size}, InventoryId: {InventoryId}",
                request.PageNumber, request.PageSize, request.InventoryId);

            // DB-level pagination with Color include and optional inventory filter
            var (items, totalCount) = await _unitOfWork.Items.GetPagedWithColorsAsync(
                request.PageNumber,
                request.PageSize,
                request.InventoryId,
                cancellationToken);

            // Map to DTOs
            var dtos = items.Select(MapToDto).ToList();

            // Build paged result with metadata
            var pagedResult = PagedResult<ItemDto>.Create(
                dtos,
                request.PageNumber,
                request.PageSize,
                totalCount);

            _logger.LogInformation("Retrieved {Count}/{Total} items (Page {Page}/{TotalPages})",
                dtos.Count, totalCount, request.PageNumber, pagedResult.TotalPages);

            return Result<PagedResult<ItemDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items");
            return Result<PagedResult<ItemDto>>.FailureResult(
                "An error occurred while retrieving items", 500);
        }
    }

    private static ItemDto MapToDto(Item entity)
    {
        return new ItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SKU = entity.SKU,
            BasePrice = entity.BasePrice,
            ColorId = entity.ColorId,
            ColorName = entity.Color?.Name,
            ImageUrl = entity.ImageUrl,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
