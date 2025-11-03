using MobileBackend.Application.Common.Interfaces;

namespace MobileBackend.Application.Common.Helpers;

/// <summary>
/// Helper service for consistent audit logging across handlers
/// Eliminates boilerplate audit log code and provides standard formatting
/// </summary>
public class AuditHelper
{
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public AuditHelper(
        IAuditService auditService, 
        ICurrentUserService currentUserService)
    {
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Logs an entity operation with consistent formatting
    /// </summary>
    /// <param name="action">Audit action constant (e.g., AuditActions.ColorCreated)</param>
    /// <param name="entityName">Entity name constant (e.g., EntityNames.Color)</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="message">Additional information message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task LogEntityOperationAsync(
        string action,
        string entityName,
        Guid entityId,
        string message,
        CancellationToken cancellationToken = default)
    {
        await _auditService.LogAsync(
            action: action,
            entityName: entityName,
            entityId: entityId,
            userId: _currentUserService.UserId ?? Guid.Empty,
            additionalInfo: message,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Logs a created entity operation with standard message format
    /// Message format: "Created entityname: displayName"
    /// </summary>
    /// <param name="entityName">Entity name (e.g., EntityNames.Color)</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="entityDisplayName">Display name (e.g., color.Name)</param>
    /// <param name="action">Audit action (e.g., AuditActions.ColorCreated)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogCreatedAsync(
        string entityName,
        Guid entityId,
        string entityDisplayName,
        string action,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            entityName,
            entityId,
            $"Created {entityName.ToLower()}: {entityDisplayName}",
            cancellationToken
        );
    }

    /// <summary>
    /// Logs an updated entity operation with standard message format
    /// Message format: "Updated entityname: displayName"
    /// </summary>
    /// <param name="entityName">Entity name (e.g., EntityNames.Color)</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="entityDisplayName">Display name (e.g., color.Name)</param>
    /// <param name="action">Audit action (e.g., AuditActions.ColorUpdated)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogUpdatedAsync(
        string entityName,
        Guid entityId,
        string entityDisplayName,
        string action,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            entityName,
            entityId,
            $"Updated {entityName.ToLower()}: {entityDisplayName}",
            cancellationToken
        );
    }

    /// <summary>
    /// Logs a deleted entity operation with standard message format
    /// Message format: "Deleted entityname: displayName"
    /// </summary>
    /// <param name="entityName">Entity name (e.g., EntityNames.Color)</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="entityDisplayName">Display name (e.g., color.Name)</param>
    /// <param name="action">Audit action (e.g., AuditActions.ColorDeleted)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogDeletedAsync(
        string entityName,
        Guid entityId,
        string entityDisplayName,
        string action,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            entityName,
            entityId,
            $"Deleted {entityName.ToLower()}: {entityDisplayName}",
            cancellationToken
        );
    }

    /// <summary>
    /// Logs a user-related operation with additional details
    /// </summary>
    /// <param name="action">Audit action (e.g., AuditActions.UserApproved)</param>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    /// <param name="details">Additional details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogUserOperationAsync(
        string action,
        Guid userId,
        string username,
        string details,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            "User",
            userId,
            $"{details} - User: {username}",
            cancellationToken
        );
    }

    /// <summary>
    /// Logs a role operation with additional context
    /// </summary>
    /// <param name="action">Audit action (e.g., AuditActions.PermissionsAssigned)</param>
    /// <param name="roleId">Role ID</param>
    /// <param name="roleName">Role name</param>
    /// <param name="details">Additional details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogRoleOperationAsync(
        string action,
        Guid roleId,
        string roleName,
        string details,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            "Role",
            roleId,
            $"{details} - Role: {roleName}",
            cancellationToken
        );
    }

    /// <summary>
    /// Logs an order operation with order number
    /// </summary>
    /// <param name="action">Audit action (e.g., AuditActions.OrderCreated)</param>
    /// <param name="orderId">Order ID</param>
    /// <param name="orderNumber">Order number</param>
    /// <param name="details">Additional details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogOrderOperationAsync(
        string action,
        Guid orderId,
        string orderNumber,
        string details,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            "Order",
            orderId,
            $"{details} - Order: {orderNumber}",
            cancellationToken
        );
    }

    /// <summary>
    /// Logs a bulk operation affecting multiple entities
    /// </summary>
    /// <param name="action">Audit action</param>
    /// <param name="entityName">Entity name</param>
    /// <param name="count">Number of entities affected</param>
    /// <param name="details">Additional details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task LogBulkOperationAsync(
        string action,
        string entityName,
        int count,
        string details,
        CancellationToken cancellationToken = default)
    {
        return LogEntityOperationAsync(
            action,
            entityName,
            Guid.Empty, // No specific entity ID for bulk operations
            $"Bulk operation: {details} - Affected {count} {entityName.ToLower()}(s)",
            cancellationToken
        );
    }
}
