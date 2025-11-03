using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Commands.DeleteInventory;

/// <summary>
/// Handler for deleting (soft delete) an inventory/warehouse
/// ? Uses BaseSoftDeleteHandler to eliminate code duplication
/// </summary>
public class DeleteInventoryCommandHandler : BaseSoftDeleteHandler<DeleteInventoryCommand, Inventory>
{
    public DeleteInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<DeleteInventoryCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(DeleteInventoryCommand command) 
        => command.Id;

    protected override async Task<Inventory?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => await UnitOfWork.Inventories.GetByIdAsync(id, cancellationToken);

    protected override void UpdateEntity(Inventory entity)
        => UnitOfWork.Inventories.Update(entity);

    protected override string GetEntityName() 
        => EntityNames.Inventory;

    protected override string GetAuditAction() 
        => AuditActions.InventoryDeleted;

    protected override string GetAuditMessage(Inventory entity)
        => $"Deleted inventory: {entity.Name}";

    // Override validation to check for existing items
    protected override async Task<Result<bool>> ValidateDeletionAsync(
        Inventory entity,
        CancellationToken cancellationToken)
    {
        // Check if inventory has items
        var itemInventories = await UnitOfWork.ItemInventories.GetByInventoryIdAsync(entity.Id, cancellationToken);
        if (itemInventories.Any())
        {
            return Result<bool>.FailureResult(
                $"Cannot delete inventory '{entity.Name}' because it contains {itemInventories.Count} items. Please remove all items first.", 
                400);
        }
        
        return Result<bool>.SuccessResult(true);
    }
}
