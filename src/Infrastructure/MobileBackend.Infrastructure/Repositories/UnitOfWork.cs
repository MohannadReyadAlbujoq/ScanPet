using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Common;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions and repository access
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repository instances
    private IUserRepository? _userRepository;
    private IRoleRepository? _roleRepository;
    private IPermissionRepository? _permissionRepository;
    private IOrderRepository? _orderRepository;
    private IOrderItemRepository? _orderItemRepository;
    private IItemRepository? _itemRepository;
    private IColorRepository? _colorRepository;
    private ILocationRepository? _locationRepository;
    private IAuditLogRepository? _auditLogRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;
    private IItemInventoryRepository? _itemInventoryRepository;
    private IInventoryRepository? _inventoryRepository; // NEW

    // Dictionary for generic repositories
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // DbContext access
    public ApplicationDbContext Context => _context;

    #region Repository Properties

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);
    public IPermissionRepository Permissions => _permissionRepository ??= new PermissionRepository(_context);
    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
    public IOrderItemRepository OrderItems => _orderItemRepository ??= new OrderItemRepository(_context);
    public IItemRepository Items => _itemRepository ??= new ItemRepository(_context);
    public IColorRepository Colors => _colorRepository ??= new ColorRepository(_context);
    public ILocationRepository Locations => _locationRepository ??= new LocationRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);
    public IItemInventoryRepository ItemInventories => _itemInventoryRepository ??= new ItemInventoryRepository(_context);
    public IInventoryRepository Inventories => _inventoryRepository ??= new InventoryRepository(_context); // NEW

    #endregion

    #region Generic Repository Access

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity);

        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new GenericRepository<TEntity>(_context);
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    #endregion

    #region Transaction Management

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts
            throw new InvalidOperationException("The entity you attempted to update was modified by another user.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Handle database update errors
            throw new InvalidOperationException("An error occurred while updating the database.", ex);
        }
    }

    public async Task<bool> SaveChangesWithTransactionAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction has been started.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }

    #endregion
}
