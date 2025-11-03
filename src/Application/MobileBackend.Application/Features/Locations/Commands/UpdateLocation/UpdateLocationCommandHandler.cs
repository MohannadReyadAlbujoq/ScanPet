using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Commands.UpdateLocation;

/// <summary>
/// Handler for updating an existing location
/// Uses BaseUpdateHandler to eliminate code duplication
/// </summary>
public class UpdateLocationCommandHandler : BaseUpdateHandler<UpdateLocationCommand, Location>
{
    public UpdateLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<UpdateLocationCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(UpdateLocationCommand command) => command.LocationId;

    protected override Task<Location?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => UnitOfWork.Locations.GetByIdAsync(id, cancellationToken);

    protected override async Task UpdateEntityPropertiesAsync(
        UpdateLocationCommand command,
        Location entity,
        CancellationToken cancellationToken)
    {
        entity.Name = command.Name;
        entity.Address = command.Address;
        entity.City = command.City;
        entity.Country = command.Country;
        entity.PostalCode = command.PostalCode;
        entity.IsActive = command.IsActive;
    }

    protected override void UpdateEntity(Location entity)
    {
        UnitOfWork.Locations.Update(entity);
    }

    protected override string GetEntityName() => EntityNames.Location;

    protected override string GetAuditAction() => AuditActions.LocationUpdated;

    protected override string CaptureOldValues(Location entity)
        => $"Name: {entity.Name}, Address: {entity.Address}, City: {entity.City}";

    protected override string CaptureNewValues(Location entity)
        => $"Name: {entity.Name}, Address: {entity.Address}, City: {entity.City}";

    // Override uniqueness validation
    protected override async Task<Result<bool>> ValidateUniquenessAsync(
        UpdateLocationCommand command,
        Location entity,
        CancellationToken cancellationToken)
    {
        var existingLocation = await UnitOfWork.Locations.GetByNameAsync(command.Name, cancellationToken);
        if (existingLocation != null && existingLocation.Id != command.LocationId)
        {
            return Result<bool>.FailureResult(ErrorMessages.AlreadyExists("Location", "name"), 409);
        }
        return Result<bool>.SuccessResult(true);
    }
}
