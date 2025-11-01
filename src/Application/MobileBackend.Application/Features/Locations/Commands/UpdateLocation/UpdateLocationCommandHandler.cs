using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Locations.Commands.UpdateLocation;

/// <summary>
/// Handler for updating an existing location
/// </summary>
public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateLocationCommandHandler> _logger;

    public UpdateLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<UpdateLocationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing location
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
            if (location == null)
            {
                return Result<bool>.FailureResult("Location not found", 404);
            }

            // Check if another location with same name exists
            var existingLocation = await _unitOfWork.Locations.GetByNameAsync(request.Name, cancellationToken);
            if (existingLocation != null && existingLocation.Id != request.LocationId)
            {
                return Result<bool>.FailureResult("Another location with this name already exists", 409);
            }

            // Store old values for audit
            var oldValues = $"Name: {location.Name}, Address: {location.Address}, City: {location.City}";
            var newValues = $"Name: {request.Name}, Address: {request.Address}, City: {request.City}";

            // Update location properties
            location.Name = request.Name;
            location.Address = request.Address;
            location.City = request.City;
            location.Country = request.Country;
            location.PostalCode = request.PostalCode;
            location.IsActive = request.IsActive;
            location.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Locations.Update(location);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogActionAsync(
                userId: _currentUserService.UserId,
                action: AuditActions.LocationUpdated,
                entityName: EntityNames.Location,
                entityId: location.Id,
                oldValues: oldValues,
                newValues: newValues,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Location updated successfully: {LocationId} - {LocationName}", location.Id, location.Name);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location: {LocationId}", request.LocationId);
            return Result<bool>.FailureResult("An error occurred while updating the location", 500);
        }
    }
}
