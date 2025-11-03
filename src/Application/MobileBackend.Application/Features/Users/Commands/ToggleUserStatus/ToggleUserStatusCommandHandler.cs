using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Commands.ToggleUserStatus;

/// <summary>
/// Handler for ToggleUserStatusCommand
/// Enables or disables a user account
/// </summary>
public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ToggleUserStatusCommandHandler> _logger;

    public ToggleUserStatusCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<ToggleUserStatusCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.FailureResult("User not found", 404);
        }

        // 2. Prevent disabling yourself
        if (request.UserId == _currentUserService.UserId && !request.IsEnabled)
        {
            return Result<bool>.FailureResult("You cannot disable your own account", 400);
        }

        // 3. Update user status
        user.IsEnabled = request.IsEnabled;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = _currentUserService.UserId;

        _unitOfWork.Users.Update(user);

        // 4. Revoke all active tokens if disabling
        if (!request.IsEnabled)
        {
            var activeTokens = await _unitOfWork.Users.GetActiveRefreshTokensAsync(request.UserId, cancellationToken);
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        // 5. Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} {Status} by {CurrentUserId}", 
            request.UserId, request.IsEnabled ? "enabled" : "disabled", _currentUserService.UserId);

        return Result<bool>.SuccessResult(true);
    }
}
