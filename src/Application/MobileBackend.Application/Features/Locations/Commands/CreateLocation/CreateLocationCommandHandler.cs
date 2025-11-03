using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Commands.CreateLocation;

/// <summary>
/// Handler for creating a new location
/// Uses BaseCreateHandler to eliminate code duplication
/// </summary>
public class CreateLocationCommandHandler : BaseCreateHandler<CreateLocationCommand, Location>
{
    public CreateLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateLocationCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override async Task<Location> CreateEntityAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        return new Location
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Address = command.Address,
            City = command.City,
            Country = command.Country,
            PostalCode = command.PostalCode,
            IsActive = command.IsActive,
            IsDeleted = false
        };
    }

    protected override Task AddEntityAsync(Location entity, CancellationToken cancellationToken)
    {
        return UnitOfWork.Locations.AddAsync(entity, cancellationToken);
    }

    protected override string GetEntityName() => EntityNames.Location;

    protected override string GetAuditAction() => AuditActions.LocationCreated;

    protected override string GetAuditMessage(Location entity)
        => $"Created location: {entity.Name} in {entity.City}, {entity.Country}";

    // Override uniqueness validation
    protected override async Task<Result<Guid>> ValidateUniquenessAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var existingLocation = await UnitOfWork.Locations.GetByNameAsync(command.Name, cancellationToken);
        if (existingLocation != null)
        {
            return Result<Guid>.FailureResult(ErrorMessages.AlreadyExists("Location", "name"), 409);
        }
        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
