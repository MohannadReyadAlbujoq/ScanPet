using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Infrastructure.Services;

/// <summary>
/// Implementation of audit logging service
/// Centralizes audit log creation logic.
/// Note: Does NOT call SaveChangesAsync — the caller's transaction handles persistence.
/// For standalone calls (e.g., from middleware), use saveImmediately: true.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public AuditService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public async Task LogAsync(
        string action,
        string entityName,
        Guid entityId,
        Guid userId,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            IpAddress = _currentUserService.IpAddress ?? "Unknown",
            UserAgent = _currentUserService.UserAgent,
            Timestamp = _dateTimeService.UtcNow,
            AdditionalInfo = additionalInfo
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task LogActionAsync(
        Guid? userId,
        string action,
        string entityName,
        Guid entityId,
        string? oldValues = null,
        string? newValues = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = _currentUserService.IpAddress ?? "Unknown",
            UserAgent = _currentUserService.UserAgent,
            Timestamp = _dateTimeService.UtcNow,
            AdditionalInfo = additionalInfo
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task LogFailedLoginAsync(
        string usernameOrEmail,
        string reason,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var newValues = $"{{\"UsernameOrEmail\":\"{usernameOrEmail}\",\"Reason\":\"{reason}\"}}";
        var additionalInfo = $"Failed login attempt for {usernameOrEmail} - {reason}";

        await LogActionAsync(
            userId: null,
            action: AuditActions.FailedLogin,
            entityName: EntityNames.User,
            entityId: Guid.Empty,
            oldValues: null,
            newValues: newValues,
            additionalInfo: additionalInfo,
            cancellationToken: cancellationToken);
    }

    public async Task LogSuccessfulLoginAsync(
        Guid userId,
        string username,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var newValues = $"{{\"Username\":\"{username}\"}}";
        var additionalInfo = $"User {username} logged in successfully";

        await LogActionAsync(
            userId: userId,
            action: AuditActions.Login,
            entityName: EntityNames.User,
            entityId: userId,
            oldValues: null,
            newValues: newValues,
            additionalInfo: additionalInfo,
            cancellationToken: cancellationToken);
    }

    public async Task LogUserRegistrationAsync(
        Guid userId,
        string username,
        string email,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        var newValues = $"{{\"Username\":\"{username}\",\"Email\":\"{email}\",\"FullName\":\"{fullName}\"}}";
        var additionalInfo = $"New user registered: {username} - Pending approval";

        await LogActionAsync(
            userId: userId,
            action: AuditActions.UserRegistered,
            entityName: EntityNames.User,
            entityId: userId,
            oldValues: null,
            newValues: newValues,
            additionalInfo: additionalInfo,
            cancellationToken: cancellationToken);
    }

    public async Task LogTokenRefreshAsync(
        Guid userId,
        string username,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var newValues = $"{{\"Username\":\"{username}\"}}";
        var additionalInfo = $"Access token refreshed for user: {username}";

        await LogActionAsync(
            userId: userId,
            action: AuditActions.TokenRefreshed,
            entityName: EntityNames.RefreshToken,
            entityId: Guid.NewGuid(), // Will be replaced with actual refresh token ID
            oldValues: null,
            newValues: newValues,
            additionalInfo: additionalInfo,
            cancellationToken: cancellationToken);
    }
}
