using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Commands.UpdateItem;

/// <summary>
/// Handler for updating an existing item
/// Uses BaseUpdateHandler to eliminate code duplication
/// </summary>
public class UpdateItemCommandHandler : BaseUpdateHandler<UpdateItemCommand, Item>
{
    public UpdateItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<UpdateItemCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(UpdateItemCommand command) => command.ItemId;

    protected override Task<Item?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => UnitOfWork.Items.GetByIdAsync(id, cancellationToken);

    protected override async Task UpdateEntityPropertiesAsync(
        UpdateItemCommand command,
        Item entity,
        CancellationToken cancellationToken)
    {
        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.SKU = command.SKU;
        entity.BasePrice = command.BasePrice;
        entity.Quantity = command.Quantity;
        entity.ColorId = command.ColorId;
        entity.ImageUrl = command.ImageUrl;
    }

    protected override void UpdateEntity(Item entity)
    {
        UnitOfWork.Items.Update(entity);
    }

    protected override string GetEntityName() => EntityNames.Item;

    protected override string GetAuditAction() => AuditActions.ItemUpdated;

    protected override string CaptureOldValues(Item entity)
        => $"Name: {entity.Name}, Price: {entity.BasePrice}, Quantity: {entity.Quantity}";

    protected override string CaptureNewValues(Item entity)
        => $"Name: {entity.Name}, Price: {entity.BasePrice}, Quantity: {entity.Quantity}";

    // Override validation to check color existence
    protected override async Task<Result<bool>> ValidateAsync(
        UpdateItemCommand command,
        Item entity,
        CancellationToken cancellationToken)
    {
        // Validate ColorId if provided
        if (command.ColorId.HasValue)
        {
            var color = await UnitOfWork.Colors.GetByIdAsync(command.ColorId.Value, cancellationToken);
            if (color == null)
            {
                return Result<bool>.FailureResult(ErrorMessages.NotFound("Color"), 404);
            }
        }
        return Result<bool>.SuccessResult(true);
    }
}
