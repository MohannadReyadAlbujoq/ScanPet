using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Auth;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for login command
/// Authenticates user and generates JWT tokens
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtService jwtService,
        ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Find user by username or email
            var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(
                request.UsernameOrEmail, 
                cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found - {UsernameOrEmail}", request.UsernameOrEmail);
                
                // Log failed attempt in audit log
                await LogFailedLoginAttempt(request.UsernameOrEmail, request.IpAddress, "User not found");
                
                return Result<LoginResponseDto>.FailureResult("Invalid username or password", 401);
            }

            // 2. Check if user is enabled
            if (!user.IsEnabled)
            {
                _logger.LogWarning("Login failed: User is disabled - {Username}", user.Username);
                await LogFailedLoginAttempt(user.Username, request.IpAddress, "User account is disabled");
                return Result<LoginResponseDto>.FailureResult("Your account has been disabled. Please contact support.", 403);
            }

            // 3. Check if user is approved
            if (!user.IsApproved)
            {
                _logger.LogWarning("Login failed: User is not approved - {Username}", user.Username);
                await LogFailedLoginAttempt(user.Username, request.IpAddress, "User account pending approval");
                return Result<LoginResponseDto>.FailureResult("Your account is pending approval. Please wait for admin approval.", 403);
            }

            // 4. Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password - {Username}", user.Username);
                await LogFailedLoginAttempt(user.Username, request.IpAddress, "Invalid password");
                return Result<LoginResponseDto>.FailureResult("Invalid username or password", 401);
            }

            // 5. Get user roles and permissions
            var roles = await _unitOfWork.Roles.GetRolesByUserIdAsync(user.Id, cancellationToken);
            var permissionsBitmask = await _unitOfWork.Permissions.GetUserPermissionBitmaskAsync(user.Id, cancellationToken);

            // 6. Generate JWT tokens
            var accessToken = _jwtService.GenerateAccessToken(
                user.Id,
                user.Username,
                user.Email,
                roles.ToList(),
                permissionsBitmask);

            var refreshTokenValue = _jwtService.GenerateRefreshToken();

            // 7. Save refresh token to database
            var refreshToken = new Domain.Entities.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                DeviceInfo = request.DeviceInfo ?? "Unknown",
                IpAddress = request.IpAddress ?? "Unknown",
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days expiry
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            // Remove old refresh tokens for this device (optional - token rotation)
            // This ensures only one active token per device
            var oldTokens = await _unitOfWork.Users.GetActiveRefreshTokensAsync(user.Id, cancellationToken);
            foreach (var oldToken in oldTokens.Where(t => t.DeviceInfo == (request.DeviceInfo ?? "Unknown")))
            {
                oldToken.IsRevoked = true;
                oldToken.RevokedAt = DateTime.UtcNow;
            }

            // Add new refresh token
            _unitOfWork.Users.AddRefreshToken(refreshToken);

            // 8. Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 9. Log successful login in audit log
            await LogSuccessfulLogin(user.Id, user.Username, request.IpAddress);

            // 10. Build response
            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // Access token expiry
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

            _logger.LogInformation("User logged in successfully - {Username}", user.Username);

            return Result<LoginResponseDto>.SuccessResult(response, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {UsernameOrEmail}", request.UsernameOrEmail);
            return Result<LoginResponseDto>.FailureResult("An error occurred during login. Please try again.", 500);
        }
    }

    /// <summary>
    /// Log failed login attempt in audit log
    /// </summary>
    private async Task LogFailedLoginAttempt(string usernameOrEmail, string? ipAddress, string reason)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = null, // No user ID for failed login
            Action = "FailedLogin",
            EntityName = "User",
            EntityId = Guid.Empty,
            OldValues = null,
            NewValues = $"{{\"UsernameOrEmail\":\"{usernameOrEmail}\",\"Reason\":\"{reason}\"}}",
            IpAddress = ipAddress ?? "Unknown",
            UserAgent = null,
            Timestamp = DateTime.UtcNow,
            AdditionalInfo = $"Failed login attempt for {usernameOrEmail} - {reason}"
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Log successful login in audit log
    /// </summary>
    private async Task LogSuccessfulLogin(Guid userId, string username, string? ipAddress)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = "Login",
            EntityName = "User",
            EntityId = userId,
            OldValues = null,
            NewValues = $"{{\"Username\":\"{username}\"}}",
            IpAddress = ipAddress ?? "Unknown",
            UserAgent = null,
            Timestamp = DateTime.UtcNow,
            AdditionalInfo = $"User {username} logged in successfully"
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();
    }
}
