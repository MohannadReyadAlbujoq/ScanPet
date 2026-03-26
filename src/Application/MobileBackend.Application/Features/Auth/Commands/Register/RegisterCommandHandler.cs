using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for user registration
/// Creates new user with default "User" role in disabled and unapproved state
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ILogger<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Check if username already exists
            if (!await _unitOfWork.Users.IsUsernameAvailableAsync(request.Username, cancellationToken))
            {
                // Check if the existing user is not activated — give a better message
                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
                if (existingUser != null && (!existingUser.IsApproved || !existingUser.IsEnabled))
                {
                    _logger.LogWarning("Registration failed: Username exists but not activated - {Username}", request.Username);
                    return Result<Guid>.FailureResult(
                        "This username is already registered but the account is not yet activated. Please contact the administrator to activate your account.", 409);
                }

                _logger.LogWarning("Registration failed: Username already exists - {Username}", request.Username);
                return Result<Guid>.FailureResult("Username is already taken", 400);
            }

            // 2. Check if email already exists
            if (!await _unitOfWork.Users.IsEmailAvailableAsync(request.Email, cancellationToken))
            {
                // Check if the existing user is not activated — give a better message
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
                if (existingUser != null && (!existingUser.IsApproved || !existingUser.IsEnabled))
                {
                    _logger.LogWarning("Registration failed: Email exists but not activated - {Email}", request.Email);
                    return Result<Guid>.FailureResult(
                        "This email is already registered but the account is not yet activated. Please contact the administrator to activate your account.", 409);
                }

                _logger.LogWarning("Registration failed: Email already exists - {Email}", request.Email);
                return Result<Guid>.FailureResult("Email is already registered", 400);
            }

            // 3. Get default "User" role
            var defaultRole = await _unitOfWork.Roles.GetRoleByNameAsync("User", cancellationToken);
            if (defaultRole == null)
            {
                _logger.LogError("Registration failed: Default 'User' role not found in database");
                return Result<Guid>.FailureResult("System configuration error. Please contact administrator.", 500);
            }

            // 4. Hash password
            var passwordHash = _passwordService.HashPassword(request.Password);

            // 5. Create user entity
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                IsEnabled = false,  // User starts disabled
                IsApproved = false, // User starts unapproved
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 6. Add user to database
            await _unitOfWork.Users.AddAsync(user, cancellationToken);

            // 7. Assign default "User" role
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = defaultRole.Id,
                AssignedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Users.AddUserRole(userRole);

            // 8. Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 9. Log registration in audit log
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Action = "UserRegistered",
                EntityName = "User",
                EntityId = user.Id,
                OldValues = null,
                NewValues = $"{{\"Username\":\"{user.Username}\",\"Email\":\"{user.Email}\",\"FullName\":\"{user.FullName}\",\"Role\":\"User\"}}",
                IpAddress = "Unknown", // Will be set by middleware
                UserAgent = null,
                Timestamp = DateTime.UtcNow,
                AdditionalInfo = $"New user registered: {user.Username} with default 'User' role - Pending approval"
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully with default 'User' role - {Username}, UserId: {UserId}", user.Username, user.Id);

            return Result<Guid>.SuccessResult(user.Id, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Username}", request.Username);
            return Result<Guid>.FailureResult("An error occurred during registration. Please try again.", 500);
        }
    }
}
