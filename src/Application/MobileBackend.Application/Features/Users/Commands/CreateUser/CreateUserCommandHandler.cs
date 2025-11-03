using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Handler for CreateUserCommand
/// Uses BaseCreateHandler to eliminate code duplication
/// Password hashing is handled in CreateEntityAsync
/// </summary>
public class CreateUserCommandHandler : BaseCreateHandler<CreateUserCommand, User>
{
    private readonly IPasswordService _passwordService;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<CreateUserCommandHandler> logger)
        : base(unitOfWork, auditService, currentUserService, dateTimeService, logger)
    {
        _passwordService = passwordService;
    }

    protected override async Task<User> CreateEntityAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        // Hash password
        var passwordHash = _passwordService.HashPassword(command.Password);

        return new User
        {
            Id = Guid.NewGuid(),
            Username = command.Username,
            Email = command.Email,
            PasswordHash = passwordHash,
            FullName = command.FullName,
            PhoneNumber = command.PhoneNumber,
            IsEnabled = false, // Disabled by default
            IsApproved = false, // Not approved by default
            IsDeleted = false
        };
    }

    protected override Task AddEntityAsync(User entity, CancellationToken cancellationToken)
    {
        return UnitOfWork.Users.AddAsync(entity, cancellationToken);
    }

    protected override string GetEntityName() => EntityNames.User;

    protected override string GetAuditAction() => AuditActions.UserCreated;

    protected override string GetAuditMessage(User entity)
        => $"User created: {entity.Username}";

    // Override uniqueness validation to check both username and email
    protected override async Task<Result<Guid>> ValidateUniquenessAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        // Check if username already exists
        var existingUser = await UnitOfWork.Users.GetByUsernameAsync(command.Username, cancellationToken);
        if (existingUser != null)
        {
            return Result<Guid>.FailureResult("Username already exists", 400);
        }

        // Check if email already exists
        var existingEmail = await UnitOfWork.Users.GetByEmailAsync(command.Email, cancellationToken);
        if (existingEmail != null)
        {
            return Result<Guid>.FailureResult("Email already exists", 400);
        }

        return Result<Guid>.SuccessResult(Guid.Empty);
    }
}
