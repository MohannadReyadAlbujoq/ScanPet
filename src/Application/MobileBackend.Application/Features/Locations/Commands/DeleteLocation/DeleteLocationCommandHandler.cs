using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Commands.DeleteLocation;

/// <summary>
/// Handler for deleting (soft delete) a location
/// Uses BaseSoftDeleteHandler to eliminate code duplication
/// </summary>
public class DeleteLocationCommandHandler : BaseSoftDeleteHandler<DeleteLocationCommand, Location>
{
    public DeleteLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<DeleteLocationCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(DeleteLocationCommand command) 
        => command.LocationId;

    protected override async Task<Location?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => await UnitOfWork.Locations.GetByIdAsync(id, cancellationToken);

    protected override void UpdateEntity(Location entity)
        => UnitOfWork.Locations.Update(entity);

    protected override string GetEntityName() 
        => EntityNames.Location;

    protected override string GetAuditAction() 
        => AuditActions.LocationDeleted;

    protected override string GetAuditMessage(Location entity)
        => $"Deleted location: {entity.Name}";
}
