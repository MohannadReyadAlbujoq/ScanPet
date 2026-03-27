using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

public class ItemRepository : GenericRepository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Item?> GetWithDetailsAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Color)
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetByColorIdAsync(Guid colorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.ColorId == colorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken = default)
    {
        // Note: Item entity doesn't have LocationId property
        // Items are not directly associated with locations
        // Locations are associated with Orders
        // If you need location-based filtering, you'll need to modify the entity structure
        return await _dbSet
            .Include(i => i.Color)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetAvailableItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Color)
            .Include(i => i.ItemInventories)
            .Where(i => i.ItemInventories.Any(ii => !ii.IsDeleted && ii.Quantity > 0))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Item>> SearchItemsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Color)
            .Where(i => i.Name.Contains(searchTerm) ||
                       (i.Description != null && i.Description.Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsItemAvailableAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Id == itemId && !i.IsDeleted)
            .AnyAsync(i => i.ItemInventories.Any(ii => !ii.IsDeleted && ii.Quantity > 0), cancellationToken);
    }

    public async Task<Item?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.SKU == sku, cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetItemsByColorAsync(Guid colorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.ColorId == colorId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetLowStockItemsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.ItemInventories)
            .Where(i => !i.IsDeleted && i.ItemInventories
                .Where(ii => !ii.IsDeleted)
                .Sum(ii => ii.Quantity) < threshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsSkuAvailableAsync(string sku, CancellationToken cancellationToken = default)
    {
        return !await _dbSet.AnyAsync(i => i.SKU == sku, cancellationToken);
    }

    // Get all items with colors included (fix N+1)
    public async Task<IEnumerable<Item>> GetAllWithColorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Color)  // ? Eager load colors
            .Where(i => !i.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    // Get single item with color included
    public async Task<Item?> GetByIdWithColorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Color)  // ? Eager load color
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
    }

    // Get items that exist in a specific inventory/section, with colors included
    public async Task<IEnumerable<Item>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Color)
            .Where(i => !i.IsDeleted && i.ItemInventories.Any(ii => ii.InventoryId == inventoryId && !ii.IsDeleted))
            .ToListAsync(cancellationToken);
    }

    // DB-level paginated query with Color include and optional inventory filter
    public async Task<(List<Item> Items, int TotalCount)> GetPagedWithColorsAsync(
        int pageNumber,
        int pageSize,
        Guid? inventoryId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Item> query = _dbSet
            .Include(i => i.Color)
            .Where(i => !i.IsDeleted);

        // Filter by inventory if specified
        if (inventoryId.HasValue)
        {
            query = query.Where(i => i.ItemInventories.Any(ii => ii.InventoryId == inventoryId.Value && !ii.IsDeleted));
        }

        // Order by name for consistent pagination
        query = query.OrderBy(i => i.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
