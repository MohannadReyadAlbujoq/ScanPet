using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Colors.Commands.DeleteColor;

/// <summary>
/// Handler for deleting (soft delete) a color
/// </summary>
public class DeleteColorCommandHandler : IRequestHandler<DeleteColorCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteColorCommandHandler> _logger;

    public DeleteColorCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<DeleteColorCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteColorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing color
            var color = await _unitOfWork.Colors.GetByIdAsync(request.ColorId);
            if (color == null)
            {
                return Result<bool>.FailureResult("Color not found", 404);
            }

            // Soft delete
            color.IsDeleted = true;
            color.DeletedAt = DateTime.UtcNow;
            color.DeletedBy = _currentUserService.UserId;
            
            _unitOfWork.Colors.Update(color);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.ColorDeleted,
                entityName: EntityNames.Color,
                entityId: color.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Deleted color: {color.Name}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Color deleted successfully: {ColorId} - {ColorName}", color.Id, color.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting color: {ColorId}", request.ColorId);
            return Result<bool>.FailureResult("An error occurred while deleting the color", 500);
        }
    }
}
