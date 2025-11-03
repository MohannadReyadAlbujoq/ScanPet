using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Commands.CreateItem;

/// <summary>
/// Handler for creating a new item
/// Uses BaseCreateHandler to eliminate code duplication
/// </summary>
public class CreateItemCommandHandler : BaseCreateHandler<CreateItemCommand, Item>
{
    public CreateItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateItemCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override async Task<Item> CreateEntityAsync(CreateItemCommand command, CancellationToken cancellationToken)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            SKU = command.SKU,
            BasePrice = command.BasePrice,
            Quantity = command.Quantity,
            ColorId = command.ColorId,
            ImageUrl = command.ImageUrl,
            IsDeleted = false
        };
    }

    protected override Task AddEntityAsync(Item entity, CancellationToken cancellationToken)
    {
        return UnitOfWork.Items.AddAsync(entity, cancellationToken);
    }

    protected override string GetEntityName() => EntityNames.Item;

    protected override string GetAuditAction() => AuditActions.ItemCreated;

    protected override string GetAuditMessage(Item entity)
        => $"Created item: {entity.Name} (SKU: {entity.SKU}, Price: {entity.BasePrice}, Qty: {entity.Quantity})";

    // Override validation to check color existence
    protected override async Task<Result<Guid>> ValidateAsync(
        CreateItemCommand command,
        CancellationToken cancellationToken)
    {
        // Validate ColorId if provided
        if (command.ColorId.HasValue)
        {
            var color = await UnitOfWork.Colors.GetByIdAsync(command.ColorId.Value, cancellationToken);
            if (color == null)
            {
                return Result<Guid>.FailureResult(ErrorMessages.NotFound("Color"), 404);
            }
        }
        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
