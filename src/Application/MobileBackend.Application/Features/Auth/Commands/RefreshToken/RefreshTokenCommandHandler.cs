using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Auth;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for refresh token command
/// Validates refresh token and generates new access token
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Find refresh token in database with user
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenWithUserAsync(request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found");
                return Result<LoginResponseDto>.FailureResult("Invalid refresh token", 401);
            }

            // 2. Check if token is revoked
            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning("Refresh token is revoked - UserId: {UserId}", refreshToken.UserId);
                return Result<LoginResponseDto>.FailureResult("Refresh token has been revoked", 401);
            }

            // 3. Check if token is expired
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token is expired - UserId: {UserId}", refreshToken.UserId);
                
                // Revoke expired token
                await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken.Id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return Result<LoginResponseDto>.FailureResult("Refresh token has expired. Please login again.", 401);
            }

            // 4. Get user
            var user = refreshToken.User;
            
            if (user == null)
            {
                _logger.LogError("User not found for refresh token - UserId: {UserId}", refreshToken.UserId);
                return Result<LoginResponseDto>.FailureResult("User not found", 404);
            }

            // 5. Check if user is still enabled and approved
            if (!user.IsEnabled || !user.IsApproved)
            {
                _logger.LogWarning("User is disabled or not approved - Username: {Username}", user.Username);
                
                // Revoke all tokens for this user
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return Result<LoginResponseDto>.FailureResult("Your account is not active", 403);
            }

            // ? FIX N+1: Get user roles and permissions in single query
            var (roles, permissionsBitmask) = await _unitOfWork.Users.GetUserRolesAndPermissionsAsync(user.Id, cancellationToken);

            // 7. Generate new access token
            var newAccessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Username,
                user.Email,
                roles.ToList(),
                permissionsBitmask);

            // 8. Generate new refresh token (token rotation for security)
            var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

            // 9. Revoke old refresh token
            await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken.Id, cancellationToken);

            // 10. Create new refresh token
            var newRefreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = newRefreshTokenValue,
                DeviceInfo = request.DeviceInfo ?? refreshToken.DeviceInfo ?? "Unknown",
                IpAddress = request.IpAddress ?? refreshToken.IpAddress ?? "Unknown",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);

            // 11. Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 12. Log token refresh in audit log
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Action = "TokenRefreshed",
                EntityName = "RefreshToken",
                EntityId = newRefreshToken.Id,
                OldValues = null,
                NewValues = $"{{\"Username\":\"{user.Username}\"}}",
                IpAddress = request.IpAddress ?? "Unknown",
                UserAgent = null,
                Timestamp = DateTime.UtcNow,
                AdditionalInfo = $"Access token refreshed for user: {user.Username}"
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 13. Build response
            var response = new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    IsEnabled = user.IsEnabled,
                    IsApproved = user.IsApproved,
                    Roles = roles.ToList(),
                    PermissionsBitmask = permissionsBitmask
                }
            };

            _logger.LogInformation("Token refreshed successfully for user: {Username}", user.Username);

            return Result<LoginResponseDto>.SuccessResult(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Result<LoginResponseDto>.FailureResult("An error occurred during token refresh. Please try again.", 500);
        }
    }
}
