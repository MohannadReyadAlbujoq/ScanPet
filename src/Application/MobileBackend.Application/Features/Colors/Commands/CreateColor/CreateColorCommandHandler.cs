using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Commands.CreateColor;

/// <summary>
/// Handler for creating a new color
/// Uses BaseCreateHandler to eliminate code duplication
/// </summary>
public class CreateColorCommandHandler : BaseCreateHandler<CreateColorCommand, Color>
{
    public CreateColorCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateColorCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
    }

    protected override async Task<Color> CreateEntityAsync(CreateColorCommand command, CancellationToken cancellationToken)
    {
        return new Color
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            RedValue = command.RedValue,
            GreenValue = command.GreenValue,
            BlueValue = command.BlueValue,
            IsDeleted = false
        };
    }

    protected override Task AddEntityAsync(Color entity, CancellationToken cancellationToken)
    {
        return UnitOfWork.Colors.AddAsync(entity, cancellationToken);
    }

    protected override string GetEntityName() => EntityNames.Color;

    protected override string GetAuditAction() => AuditActions.ColorCreated;

    protected override string GetAuditMessage(Color entity)
        => $"Created color: {entity.Name} (RGB: {entity.RedValue}, {entity.GreenValue}, {entity.BlueValue})";

    // Override uniqueness validation
    protected override async Task<Result<Guid>> ValidateUniquenessAsync(
        CreateColorCommand command,
        CancellationToken cancellationToken)
    {
        var existingColor = await UnitOfWork.Colors.GetByNameAsync(command.Name, cancellationToken);
        if (existingColor != null)
        {
            return Result<Guid>.FailureResult(ErrorMessages.AlreadyExists("Color", "name"), 409);
        }
        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
