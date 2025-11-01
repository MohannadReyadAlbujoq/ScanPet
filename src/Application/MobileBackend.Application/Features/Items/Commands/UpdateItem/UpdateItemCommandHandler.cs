using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Items.Commands.UpdateItem;

/// <summary>
/// Handler for updating an existing item
/// </summary>
public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateItemCommandHandler> _logger;

    public UpdateItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<UpdateItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing item
            var item = await _unitOfWork.Items.GetByIdAsync(request.ItemId);
            if (item == null)
            {
                return Result<bool>.FailureResult("Item not found", 404);
            }

            // Validate ColorId if provided
            if (request.ColorId.HasValue)
            {
                var color = await _unitOfWork.Colors.GetByIdAsync(request.ColorId.Value);
                if (color == null)
                {
                    return Result<bool>.FailureResult("Color not found", 404);
                }
            }

            // Store old values for audit
            var oldValues = $"Name: {item.Name}, Price: {item.BasePrice}, Quantity: {item.Quantity}";
            var newValues = $"Name: {request.Name}, Price: {request.BasePrice}, Quantity: {request.Quantity}";

            // Update item properties
            item.Name = request.Name;
            item.Description = request.Description;
            item.SKU = request.SKU;
            item.BasePrice = request.BasePrice;
            item.Quantity = request.Quantity;
            item.ColorId = request.ColorId;
            item.ImageUrl = request.ImageUrl;
            item.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Items.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogActionAsync(
                userId: _currentUserService.UserId,
                action: AuditActions.ItemUpdated,
                entityName: EntityNames.Item,
                entityId: item.Id,
                oldValues: oldValues,
                newValues: newValues,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Item updated successfully: {ItemId} - {ItemName}", item.Id, item.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating item: {ItemId}", request.ItemId);
            return Result<bool>.FailureResult("An error occurred while updating the item", 500);
        }
    }
}
