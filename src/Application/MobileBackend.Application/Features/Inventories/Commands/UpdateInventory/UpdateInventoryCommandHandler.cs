using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Commands.UpdateInventory;

/// <summary>
/// Handler for updating an inventory/warehouse
/// ? Uses BaseUpdateHandler to eliminate code duplication
/// </summary>
public class UpdateInventoryCommandHandler : BaseUpdateHandler<UpdateInventoryCommand, Inventory>
{
    public UpdateInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<UpdateInventoryCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(UpdateInventoryCommand command) => command.Id;

    protected override Task<Inventory?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => UnitOfWork.Inventories.GetByIdAsync(id, cancellationToken);

    protected override async Task UpdateEntityPropertiesAsync(
        UpdateInventoryCommand command,
        Inventory entity,
        CancellationToken cancellationToken)
    {
        entity.Name = command.Name;
        entity.Location = command.Location;
        entity.Description = command.Description;
        entity.IsActive = command.IsActive;
    }

    protected override void UpdateEntity(Inventory entity)
    {
        UnitOfWork.Inventories.Update(entity);
    }

    protected override string GetEntityName() => EntityNames.Inventory;

    protected override string GetAuditAction() => AuditActions.InventoryUpdated;

    protected override string CaptureOldValues(Inventory entity)
        => $"Name: {entity.Name}, Location: {entity.Location}, IsActive: {entity.IsActive}";

    protected override string CaptureNewValues(Inventory entity)
        => $"Name: {entity.Name}, Location: {entity.Location}, IsActive: {entity.IsActive}";

    // Override uniqueness validation
    protected override async Task<Result<bool>> ValidateUniquenessAsync(
        UpdateInventoryCommand command,
        Inventory entity,
        CancellationToken cancellationToken)
    {
        var existingInventory = await UnitOfWork.Inventories.GetByNameAsync(command.Name, cancellationToken);
        if (existingInventory != null && existingInventory.Id != command.Id)
        {
            return Result<bool>.FailureResult(ErrorMessages.AlreadyExists("Inventory", "name"), 409);
        }
        return Result<bool>.SuccessResult(true);
    }
}
