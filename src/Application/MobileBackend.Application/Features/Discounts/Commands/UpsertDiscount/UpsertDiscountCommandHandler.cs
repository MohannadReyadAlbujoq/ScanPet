using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Domain.Enums;

namespace MobileBackend.Application.Features.Discounts.Commands.UpsertDiscount;

public class UpsertDiscountCommandHandler : IRequestHandler<UpsertDiscountCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpsertDiscountCommandHandler> _logger;

    public UpsertDiscountCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<UpsertDiscountCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpsertDiscountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.IsDefined(typeof(DiscountScope), request.Scope))
                return Result<Guid>.FailureResult("Invalid Scope. Use 0=Item, 1=ItemInventory, 2=ItemLocation", 400);

            var scope = (DiscountScope)request.Scope;

            if (request.ItemId == Guid.Empty)
                return Result<Guid>.FailureResult("ItemId is required", 400);

            if (scope == DiscountScope.ItemInventory && (!request.InventoryId.HasValue || request.InventoryId == Guid.Empty))
                return Result<Guid>.FailureResult("InventoryId is required for ItemInventory scope", 400);

            if (scope == DiscountScope.ItemLocation && (!request.LocationId.HasValue || request.LocationId == Guid.Empty))
                return Result<Guid>.FailureResult("LocationId is required for ItemLocation scope", 400);

            if (request.StartsAt.HasValue && request.ExpiresAt.HasValue && request.ExpiresAt < request.StartsAt)
                return Result<Guid>.FailureResult("ExpiresAt must be greater than or equal to StartsAt", 400);

            // v5: Treat 0 as null
            var amount = request.Amount;
            if (amount.HasValue && amount.Value == 0m) amount = null;

            Discount entity;
            if (request.Id.HasValue && request.Id.Value != Guid.Empty)
            {
                var existing = await _unitOfWork.Discounts.GetByIdAsync(request.Id.Value, cancellationToken);
                if (existing == null)
                    return Result<Guid>.FailureResult("Discount not found", 404);
                entity = existing;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUserService.UserId;
            }
            else
            {
                entity = new Discount
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.UserId
                };
                await _unitOfWork.Discounts.AddAsync(entity, cancellationToken);
            }

            entity.Scope = scope;
            entity.ItemId = request.ItemId;
            entity.InventoryId = scope == DiscountScope.ItemInventory ? request.InventoryId : null;
            entity.LocationId = scope == DiscountScope.ItemLocation ? request.LocationId : null;
            entity.Amount = amount;
            entity.Label = request.Label;
            entity.IsStackable = request.IsStackable;
            entity.IsRevertable = request.IsRevertable;
            entity.StartsAt = request.StartsAt;
            entity.ExpiresAt = request.ExpiresAt;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.SuccessResult(entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting discount");
            return Result<Guid>.FailureResult("An error occurred while saving the discount", 500);
        }
    }
}
