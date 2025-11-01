using MediatR;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Handler for CreateUserCommand
/// Creates a new user account (admin only)
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username already exists
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
        {
            return Result<Guid>.FailureResult("Username already exists", 400);
        }

        // Check if email already exists
        var existingEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail != null)
        {
            return Result<Guid>.FailureResult("Email already exists", 400);
        }

        // Hash password
        var passwordHash = _passwordService.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsEnabled = false, // Disabled by default
            IsApproved = false, // Not approved by default
            CreatedAt = _dateTimeService.UtcNow,
            CreatedBy = _currentUserService.UserId ?? Guid.Empty
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogAsync(
            AuditActions.UserCreated,
            EntityNames.User,
            user.Id,
            _currentUserService.UserId ?? Guid.Empty,
            $"User created: {user.Username}",
            cancellationToken);

        return Result<Guid>.SuccessResult(user.Id, 201);
    }
}
