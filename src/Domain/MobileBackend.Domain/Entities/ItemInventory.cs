using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Many-to-many relationship between Items and Inventories (Warehouses)
/// Tracks quantity of each item at each inventory/warehouse
/// </summary>
public class ItemInventory : BaseEntity, ISoftDelete
{
    /// <summary>
    /// Item ID
    /// </summary>
    public Guid ItemId { get; set; }
    
    /// <summary>
    /// Inventory ID (Warehouse/Storage Facility)
    /// </summary>
    public Guid InventoryId { get; set; }
    
    /// <summary>
    /// Quantity of this item at this inventory
    /// </summary>
    public int Quantity { get; set; } = 0;
    
    /// <summary>
    /// Minimum quantity threshold for alerts
    /// </summary>
    public int? MinimumQuantity { get; set; }
    
    /// <summary>
    /// Maximum quantity capacity at this inventory
    /// </summary>
    public int? MaximumQuantity { get; set; }
    
    /// <summary>
    /// Notes about this inventory entry
    /// </summary>
    public string? Notes { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual Item Item { get; set; } = null!;
    public virtual Inventory Inventory { get; set; } = null!;
}
