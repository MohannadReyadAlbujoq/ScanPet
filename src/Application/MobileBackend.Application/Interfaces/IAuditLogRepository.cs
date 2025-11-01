using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for AuditLog-specific operations
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default);
    Task LogAsync(string action, string entityName, Guid entityId, Guid userId, string? details = null, CancellationToken cancellationToken = default);
}
