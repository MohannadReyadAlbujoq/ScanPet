using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Inventory operations
/// </summary>
public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
{
    public InventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<Inventory>> GetActiveInventoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.ItemInventories)
            .Include(i => i.ParentLocation)
            .Where(i => i.IsActive && !i.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<Inventory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Name == name && !i.IsDeleted, cancellationToken);
    }

    public async Task<List<Inventory>> GetWithItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.ItemInventories)
            .Include(i => i.ParentLocation)
            .Where(i => !i.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all inventories with item counts (efficient - single query with aggregation)
    /// ? Prevents N+1 query problem - SQL COUNT aggregation
    /// </summary>
    public async Task<IEnumerable<(Inventory Inventory, int ItemCount)>> GetAllWithItemCountsAsync(CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .Include(i => i.ParentLocation)
            .Where(i => !i.IsDeleted)
            .Select(i => new
            {
                Inventory = i,
                ItemCount = i.ItemInventories.Count(ii => !ii.IsDeleted)  // ? Efficient SQL COUNT
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Inventory, r.ItemCount));
    }

    /// <summary>
    /// Get single inventory with item count (efficient - single query with aggregation)
    /// ? Prevents N+1 query problem - SQL COUNT aggregation
    /// </summary>
    public async Task<(Inventory? Inventory, int ItemCount)> GetByIdWithItemCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .Include(i => i.ParentLocation)
            .Where(i => i.Id == id && !i.IsDeleted)
            .Select(i => new
            {
                Inventory = i,
                ItemCount = i.ItemInventories.Count(ii => !ii.IsDeleted)  // ? Efficient SQL COUNT
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return (null, 0);

        return (result.Inventory, result.ItemCount);
    }

    /// <summary>
    /// Get inventories by parent location ID
    /// </summary>
    public async Task<List<Inventory>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.ParentLocation)
            .Where(i => i.LocationId == locationId && !i.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get inventories by location ID with item counts
    /// </summary>
    public async Task<IEnumerable<(Inventory Inventory, int ItemCount)>> GetByLocationIdWithItemCountsAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        var results = await _dbSet
            .Include(i => i.ParentLocation)
            .Where(i => i.LocationId == locationId && !i.IsDeleted)
            .Select(i => new
            {
                Inventory = i,
                ItemCount = i.ItemInventories.Count(ii => !ii.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Inventory, r.ItemCount));
    }
}
