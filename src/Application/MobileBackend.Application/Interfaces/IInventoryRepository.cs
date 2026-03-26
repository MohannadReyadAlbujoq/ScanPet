using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for Inventory operations
/// </summary>
public interface IInventoryRepository : IRepository<Inventory>
{
    /// <summary>
    /// Get all active inventories
    /// </summary>
    Task<List<Inventory>> GetActiveInventoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get inventory by name
    /// </summary>
    Task<Inventory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get inventories with their items (eager loaded)
    /// </summary>
    Task<List<Inventory>> GetWithItemsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all inventories with item counts (efficient - single query with aggregation)
    /// Prevents N+1 query problem
    /// </summary>
    Task<IEnumerable<(Inventory Inventory, int ItemCount)>> GetAllWithItemCountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get single inventory with item count (efficient - single query with aggregation)
    /// Prevents N+1 query problem
    /// </summary>
    Task<(Inventory? Inventory, int ItemCount)> GetByIdWithItemCountAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get inventories by parent location ID
    /// </summary>
    Task<List<Inventory>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get inventories by location ID with item counts
    /// </summary>
    Task<IEnumerable<(Inventory Inventory, int ItemCount)>> GetByLocationIdWithItemCountsAsync(Guid locationId, CancellationToken cancellationToken = default);
}
