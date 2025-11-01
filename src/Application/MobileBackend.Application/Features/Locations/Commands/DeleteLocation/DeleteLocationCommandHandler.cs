using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Locations.Commands.DeleteLocation;

/// <summary>
/// Handler for deleting (soft delete) a location
/// </summary>
public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteLocationCommandHandler> _logger;

    public DeleteLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<DeleteLocationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing location
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
            if (location == null)
            {
                return Result<bool>.FailureResult("Location not found", 404);
            }

            // Soft delete
            location.IsDeleted = true;
            location.DeletedAt = DateTime.UtcNow;
            location.DeletedBy = _currentUserService.UserId;

            _unitOfWork.Locations.Update(location);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.LocationDeleted,
                entityName: EntityNames.Location,
                entityId: location.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Deleted location: {location.Name}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Location deleted successfully: {LocationId} - {LocationName}", location.Id, location.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting location: {LocationId}", request.LocationId);
            return Result<bool>.FailureResult("An error occurred while deleting the location", 500);
        }
    }
}
