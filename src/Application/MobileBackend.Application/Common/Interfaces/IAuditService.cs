using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Common.Interfaces;

/// <summary>
/// Service for creating audit log entries
/// Centralizes audit logging logic to avoid duplication
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log a general action
    /// </summary>
    Task LogAsync(
        string action,
        string entityName,
        Guid entityId,
        Guid userId,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log an action with old and new values
    /// </summary>
    Task LogActionAsync(
        Guid? userId,
        string action,
        string entityName,
        Guid entityId,
        string? oldValues = null,
        string? newValues = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a failed login attempt
    /// </summary>
    Task LogFailedLoginAsync(
        string usernameOrEmail,
        string reason,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a successful login
    /// </summary>
    Task LogSuccessfulLoginAsync(
        Guid userId,
        string username,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log user registration
    /// </summary>
    Task LogUserRegistrationAsync(
        Guid userId,
        string username,
        string email,
        string fullName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log token refresh
    /// </summary>
    Task LogTokenRefreshAsync(
        Guid userId,
        string username,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
