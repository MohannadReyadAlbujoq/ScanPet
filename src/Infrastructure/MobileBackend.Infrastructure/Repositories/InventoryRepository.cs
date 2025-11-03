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
}
