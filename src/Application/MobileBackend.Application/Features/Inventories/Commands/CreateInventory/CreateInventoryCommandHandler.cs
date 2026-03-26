using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Inventories.Commands.CreateInventory;

/// <summary>
/// Handler for creating a new inventory/warehouse
/// ? Uses BaseCreateHandler to eliminate code duplication
/// </summary>
public class CreateInventoryCommandHandler : BaseCreateHandler<CreateInventoryCommand, Inventory>
{
    public CreateInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateInventoryCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override async Task<Inventory> CreateEntityAsync(CreateInventoryCommand command, CancellationToken cancellationToken)
    {
        return new Inventory
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Location = command.Location,
            Description = command.Description,
            IsActive = command.IsActive,
            LocationId = command.LocationId,
            IsDeleted = false
        };
    }

    protected override Task AddEntityAsync(Inventory entity, CancellationToken cancellationToken)
    {
        return UnitOfWork.Inventories.AddAsync(entity, cancellationToken);
    }

    protected override string GetEntityName() => EntityNames.Inventory;

    protected override string GetAuditAction() => AuditActions.InventoryCreated;

    protected override string GetAuditMessage(Inventory entity)
        => $"Created inventory: {entity.Name} at {entity.Location ?? "standalone"}" +
           (entity.LocationId.HasValue ? $" (section of location {entity.LocationId})" : "");

    // Override uniqueness validation
    protected override async Task<Result<Guid>> ValidateUniquenessAsync(
        CreateInventoryCommand command,
        CancellationToken cancellationToken)
    {
        var existingInventory = await UnitOfWork.Inventories.GetByNameAsync(command.Name, cancellationToken);
        if (existingInventory != null)
        {
            return Result<Guid>.FailureResult(ErrorMessages.AlreadyExists("Inventory", "name"), 409);
        }

        // Validate LocationId if provided
        if (command.LocationId.HasValue)
        {
            var location = await UnitOfWork.Locations.GetByIdAsync(command.LocationId.Value, cancellationToken);
            if (location == null)
            {
                return Result<Guid>.FailureResult(ErrorMessages.NotFound("Location"), 404);
            }
        }

        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
