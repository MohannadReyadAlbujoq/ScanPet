using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Commands.DeleteColor;

/// <summary>
/// Handler for deleting (soft delete) a color
/// Uses BaseSoftDeleteHandler to eliminate code duplication
/// </summary>
public class DeleteColorCommandHandler : BaseSoftDeleteHandler<DeleteColorCommand, Color>
{
    public DeleteColorCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<DeleteColorCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(DeleteColorCommand command) 
        => command.ColorId;

    protected override async Task<Color?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => await UnitOfWork.Colors.GetByIdAsync(id, cancellationToken);

    protected override void UpdateEntity(Color entity)
        => UnitOfWork.Colors.Update(entity);

    protected override string GetEntityName() 
        => EntityNames.Color;

    protected override string GetAuditAction() 
        => AuditActions.ColorDeleted;

    protected override string GetAuditMessage(Color entity)
        => $"Deleted color: {entity.Name}";
}
