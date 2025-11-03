using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Commands.DeleteItem;

/// <summary>
/// Handler for deleting (soft delete) an item
/// Uses BaseSoftDeleteHandler to eliminate code duplication
/// </summary>
public class DeleteItemCommandHandler : BaseSoftDeleteHandler<DeleteItemCommand, Item>
{
    public DeleteItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<DeleteItemCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(DeleteItemCommand command) 
        => command.ItemId;

    protected override async Task<Item?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => await UnitOfWork.Items.GetByIdAsync(id, cancellationToken);

    protected override void UpdateEntity(Item entity)
        => UnitOfWork.Items.Update(entity);

    protected override string GetEntityName() 
        => EntityNames.Item;

    protected override string GetAuditAction() 
        => AuditActions.ItemDeleted;

    protected override string GetAuditMessage(Item entity)
        => $"Deleted item: {entity.Name}";
}
