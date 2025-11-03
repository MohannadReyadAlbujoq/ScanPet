using Microsoft.EntityFrameworkCore;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;
using MobileBackend.Infrastructure.Data;

namespace MobileBackend.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ItemInventory operations
/// </summary>
public class ItemInventoryRepository : GenericRepository<ItemInventory>, IItemInventoryRepository
{
    public ItemInventoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<ItemInventory>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ii => ii.Item)
            .Include(ii => ii.Inventory)
            .Where(ii => ii.ItemId == itemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ItemInventory>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ii => ii.Item)
            .Include(ii => ii.Inventory)
            .Where(ii => ii.InventoryId == inventoryId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ItemInventory?> GetByItemAndInventoryAsync(Guid itemId, Guid inventoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ii => ii.Item)
            .Include(ii => ii.Inventory)
            .FirstOrDefaultAsync(ii => ii.ItemId == itemId && ii.InventoryId == inventoryId, cancellationToken);
    }

    public async Task<int> GetTotalQuantityForItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ii => ii.ItemId == itemId)
            .SumAsync(ii => ii.Quantity, cancellationToken);
    }

    public async Task<List<ItemInventory>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ii => ii.Item)
            .Include(ii => ii.Inventory)
            .Where(ii => ii.MinimumQuantity.HasValue && ii.Quantity <= ii.MinimumQuantity.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TransferInventoryAsync(
        Guid itemId, 
        Guid fromInventoryId, 
        Guid toInventoryId, 
        int quantity, 
        CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return false;

        var fromInventory = await GetByItemAndInventoryAsync(itemId, fromInventoryId, cancellationToken);
        if (fromInventory == null || fromInventory.Quantity < quantity)
            return false;

        var toInventory = await GetByItemAndInventoryAsync(itemId, toInventoryId, cancellationToken);

        // Decrease from source
        fromInventory.Quantity -= quantity;
        fromInventory.UpdatedAt = DateTime.UtcNow;
        Update(fromInventory);

        // Increase at destination (or create new entry)
        if (toInventory != null)
        {
            toInventory.Quantity += quantity;
            toInventory.UpdatedAt = DateTime.UtcNow;
            Update(toInventory);
        }
        else
        {
            await AddAsync(new ItemInventory
            {
                ItemId = itemId,
                InventoryId = toInventoryId,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        return true;
    }

    public async Task<bool> AdjustInventoryAsync(
        Guid itemId, 
        Guid inventoryId, 
        int quantityChange, 
        CancellationToken cancellationToken = default)
    {
        var inventory = await GetByItemAndInventoryAsync(itemId, inventoryId, cancellationToken);

        if (inventory == null)
        {
            // Create new inventory entry if doesn't exist
            if (quantityChange <= 0)
                return false;

            await AddAsync(new ItemInventory
            {
                ItemId = itemId,
                InventoryId = inventoryId,
                Quantity = quantityChange,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        else
        {
            var newQuantity = inventory.Quantity + quantityChange;
            if (newQuantity < 0)
                return false;

            inventory.Quantity = newQuantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            Update(inventory);
        }

        return true;
    }
}
