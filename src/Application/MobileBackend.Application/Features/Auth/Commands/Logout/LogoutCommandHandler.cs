using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Handler for user logout - revokes refresh token
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Result<bool>.FailureResult("User not authenticated", 401);
            }

            // If refresh token is provided, revoke it specifically
            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken, cancellationToken);
                if (refreshToken != null && refreshToken.UserId == userId.Value)
                {
                    refreshToken.IsRevoked = true;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    _unitOfWork.RefreshTokens.Update(refreshToken);
                }
            }
            else
            {
                // If no refresh token provided, revoke all active tokens for this user
                var activeTokens = await _unitOfWork.RefreshTokens.GetActiveTokensByUserIdAsync(userId.Value, cancellationToken);
                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    _unitOfWork.RefreshTokens.Update(token);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.Logout,
                entityName: "User",
                entityId: userId.Value,
                userId: userId.Value,
                additionalInfo: $"User logged out from IP: {_currentUserService.IpAddress}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("User {UserId} logged out successfully", userId.Value);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", _currentUserService.UserId);
            return Result<bool>.FailureResult("An error occurred during logout", 500);
        }
    }
}
