using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Interfaces;

/// <summary>
/// Repository interface for ItemInventory operations
/// </summary>
public interface IItemInventoryRepository : IRepository<ItemInventory>
{
    /// <summary>
    /// Get all inventory entries for a specific item
    /// </summary>
    Task<List<ItemInventory>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all inventory entries for a specific inventory/warehouse
    /// </summary>
    Task<List<ItemInventory>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get inventory entry for specific item at specific inventory/warehouse
    /// </summary>
    Task<ItemInventory?> GetByItemAndInventoryAsync(Guid itemId, Guid inventoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get total quantity of an item across all inventories
    /// </summary>
    Task<int> GetTotalQuantityForItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get items with low stock at any inventory
    /// </summary>
    Task<List<ItemInventory>> GetLowStockItemsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transfer inventory from one warehouse to another
    /// </summary>
    Task<bool> TransferInventoryAsync(
        Guid itemId, 
        Guid fromInventoryId, 
        Guid toInventoryId, 
        int quantity, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adjust inventory quantity at a warehouse (increase or decrease)
    /// </summary>
    Task<bool> AdjustInventoryAsync(
        Guid itemId, 
        Guid inventoryId, 
        int quantityChange, 
        CancellationToken cancellationToken = default);
}
