using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Discounts;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Discounts.Queries.GetDiscountsForItem;

public class GetDiscountsForItemQuery : IRequest<Result<List<DiscountDto>>>
{
    public Guid ItemId { get; set; }
}

public class GetDiscountsForItemQueryHandler : IRequestHandler<GetDiscountsForItemQuery, Result<List<DiscountDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDiscountsForItemQueryHandler> _logger;

    public GetDiscountsForItemQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDiscountsForItemQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<DiscountDto>>> Handle(GetDiscountsForItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await _unitOfWork.Discounts.GetByItemIdAsync(request.ItemId, cancellationToken);
            var list = rows.Select(d => new DiscountDto
            {
                Id = d.Id,
                Scope = (int)d.Scope,
                ScopeName = d.Scope.ToString(),
                ItemId = d.ItemId,
                InventoryId = d.InventoryId,
                LocationId = d.LocationId,
                Amount = (d.Amount.HasValue && d.Amount.Value == 0m) ? null : d.Amount,
                Label = d.Label,
                IsStackable = d.IsStackable,
                IsRevertable = d.IsRevertable,
                StartsAt = d.StartsAt,
                ExpiresAt = d.ExpiresAt,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();
            return Result<List<DiscountDto>>.SuccessResult(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving discounts for item {ItemId}", request.ItemId);
            return Result<List<DiscountDto>>.FailureResult("An error occurred while retrieving discounts", 500);
        }
    }
}
