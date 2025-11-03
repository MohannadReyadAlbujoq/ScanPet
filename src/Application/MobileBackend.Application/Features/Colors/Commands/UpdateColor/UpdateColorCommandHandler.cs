using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Commands.UpdateColor;

/// <summary>
/// Handler for updating an existing color
/// Uses BaseUpdateHandler to eliminate code duplication
/// </summary>
public class UpdateColorCommandHandler : BaseUpdateHandler<UpdateColorCommand, Color>
{
    public UpdateColorCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<UpdateColorCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override Guid GetEntityId(UpdateColorCommand command) => command.ColorId;

    protected override Task<Color?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
        => UnitOfWork.Colors.GetByIdAsync(id, cancellationToken);

    protected override async Task UpdateEntityPropertiesAsync(
        UpdateColorCommand command,
        Color entity,
        CancellationToken cancellationToken)
    {
        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.RedValue = command.RedValue;
        entity.GreenValue = command.GreenValue;
        entity.BlueValue = command.BlueValue;
    }

    protected override void UpdateEntity(Color entity)
    {
        UnitOfWork.Colors.Update(entity);
    }

    protected override string GetEntityName() => EntityNames.Color;

    protected override string GetAuditAction() => AuditActions.ColorUpdated;

    protected override string CaptureOldValues(Color entity)
        => $"Name: {entity.Name}, RGB: ({entity.RedValue}, {entity.GreenValue}, {entity.BlueValue})";

    protected override string CaptureNewValues(Color entity)
        => $"Name: {entity.Name}, RGB: ({entity.RedValue}, {entity.GreenValue}, {entity.BlueValue})";

    // Override uniqueness validation
    protected override async Task<Result<bool>> ValidateUniquenessAsync(
        UpdateColorCommand command,
        Color entity,
        CancellationToken cancellationToken)
    {
        var existingColor = await UnitOfWork.Colors.GetByNameAsync(command.Name, cancellationToken);
        if (existingColor != null && existingColor.Id != command.ColorId)
        {
            return Result<bool>.FailureResult(ErrorMessages.AlreadyExists("Color", "name"), 409);
        }
        return Result<bool>.SuccessResult(true);
    }
}
