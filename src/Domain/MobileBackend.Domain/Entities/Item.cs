using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

public class Item : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SKU { get; set; }
    public decimal BasePrice { get; set; }
    
    /// <summary>
    /// DEPRECATED: Use ItemInventories collection for actual quantities per inventory
    /// This is kept for backward compatibility and represents total quantity across all inventories
    /// </summary>
    [Obsolete("Use GetTotalQuantity() or ItemInventories collection instead")]
    public int Quantity { get; set; } = 0;
    
    public Guid? ColorId { get; set; }
    public string? ImageUrl { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual Color? Color { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    /// <summary>
    /// Item quantities across different inventories/warehouses
    /// </summary>
    public virtual ICollection<ItemInventory> ItemInventories { get; set; } = new List<ItemInventory>();
    
    /// <summary>
    /// Gets the total quantity across all inventories
    /// </summary>
    public int GetTotalQuantity() => ItemInventories.Where(i => !i.IsDeleted).Sum(i => i.Quantity);
    
    /// <summary>
    /// Gets quantity at a specific inventory
    /// </summary>
    public int GetQuantityAtInventory(Guid inventoryId) => 
        ItemInventories
            .Where(i => i.InventoryId == inventoryId && !i.IsDeleted)
            .Sum(i => i.Quantity);
}
