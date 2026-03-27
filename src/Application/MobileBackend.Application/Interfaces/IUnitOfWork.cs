using System.Data;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// Coordinates repository operations and manages transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    IItemRepository Items { get; }
    IColorRepository Colors { get; }
    ILocationRepository Locations { get; }
    IAuditLogRepository AuditLogs { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IItemInventoryRepository ItemInventories { get; }
    IInventoryRepository Inventories { get; }

    // Transaction management
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
