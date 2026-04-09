using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Commands.SetDefaultInventory;

/// <summary>
/// Handler for setting a user's default inventory
/// </summary>
public class SetDefaultInventoryCommandHandler : IRequestHandler<SetDefaultInventoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetDefaultInventoryCommandHandler> _logger;

    public SetDefaultInventoryCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<SetDefaultInventoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SetDefaultInventoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<bool>.FailureResult(ErrorMessages.NotFound(EntityNames.User), 404);
            }

            // Validate inventory exists if provided
            if (request.DefaultInventoryId.HasValue)
            {
                var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.DefaultInventoryId.Value, cancellationToken);
                if (inventory == null)
                {
                    return Result<bool>.FailureResult(ErrorMessages.NotFound(EntityNames.Inventory), 404);
                }
            }

            user.DefaultInventoryId = request.DefaultInventoryId;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} default inventory set to {InventoryId}", 
                request.UserId, request.DefaultInventoryId);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default inventory for user {UserId}", request.UserId);
            return Result<bool>.FailureResult(ErrorMessages.UpdateFailed(EntityNames.User), 500);
        }
    }
}
