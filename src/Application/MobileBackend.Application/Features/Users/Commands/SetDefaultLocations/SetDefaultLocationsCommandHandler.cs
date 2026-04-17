using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Users.Commands.SetDefaultLocations;

/// <summary>
/// Handler for setting a user's default locations (replaces the whole set)
/// </summary>
public class SetDefaultLocationsCommandHandler : IRequestHandler<SetDefaultLocationsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetDefaultLocationsCommandHandler> _logger;

    public SetDefaultLocationsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<SetDefaultLocationsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SetDefaultLocationsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdWithRolesAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<bool>.FailureResult(ErrorMessages.NotFound(EntityNames.User), 404);
            }

            // Validate all provided location IDs exist
            foreach (var locationId in request.DefaultLocationIds)
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(locationId, cancellationToken);
                if (location == null)
                {
                    return Result<bool>.FailureResult($"Location with ID {locationId} not found", 404);
                }
            }

            // Clear existing default locations and replace with new ones
            user.DefaultLocations.Clear();

            foreach (var locationId in request.DefaultLocationIds)
            {
                user.DefaultLocations.Add(new UserDefaultLocation
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LocationId = locationId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} default locations updated to [{LocationIds}]",
                request.UserId, string.Join(", ", request.DefaultLocationIds));

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default locations for user {UserId}", request.UserId);
            return Result<bool>.FailureResult(ErrorMessages.UpdateFailed(EntityNames.User), 500);
        }
    }
}
