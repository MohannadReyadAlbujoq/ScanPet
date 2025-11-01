using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Colors.Commands.UpdateColor;

/// <summary>
/// Handler for updating an existing color
/// </summary>
public class UpdateColorCommandHandler : IRequestHandler<UpdateColorCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateColorCommandHandler> _logger;

    public UpdateColorCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<UpdateColorCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateColorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing color
            var color = await _unitOfWork.Colors.GetByIdAsync(request.ColorId);
            if (color == null)
            {
                return Result<bool>.FailureResult("Color not found", 404);
            }

            // Check if another color with same name exists
            var existingColor = await _unitOfWork.Colors.GetByNameAsync(request.Name, cancellationToken);
            if (existingColor != null && existingColor.Id != request.ColorId)
            {
                return Result<bool>.FailureResult("Another color with this name already exists", 409);
            }

            // Update color properties
            var oldValues = $"Name: {color.Name}, RGB: ({color.RedValue}, {color.GreenValue}, {color.BlueValue})";
            var newValues = $"Name: {request.Name}, RGB: ({request.RedValue}, {request.GreenValue}, {request.BlueValue})";
            
            color.Name = request.Name;
            color.Description = request.Description;
            color.RedValue = request.RedValue;
            color.GreenValue = request.GreenValue;
            color.BlueValue = request.BlueValue;
            color.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Colors.Update(color);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogActionAsync(
                userId: _currentUserService.UserId,
                action: AuditActions.ColorUpdated,
                entityName: EntityNames.Color,
                entityId: color.Id,
                oldValues: oldValues,
                newValues: newValues,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Color updated successfully: {ColorId} - {ColorName}", color.Id, color.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating color: {ColorId}", request.ColorId);
            return Result<bool>.FailureResult("An error occurred while updating the color", 500);
        }
    }
}
