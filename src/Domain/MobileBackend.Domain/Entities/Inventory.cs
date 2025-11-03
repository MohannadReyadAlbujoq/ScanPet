using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Represents a warehouse or storage facility (inventory location)
/// Separate from Location which represents order delivery locations
/// </summary>
public class Inventory : BaseEntity, ISoftDelete
{
    /// <summary>
    /// Name of the warehouse/inventory location
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Physical location/address of the warehouse
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Description or notes about this inventory/warehouse
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Whether this inventory location is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    /// <summary>
    /// Items stored in this inventory with their quantities
    /// </summary>
    public virtual ICollection<ItemInventory> ItemInventories { get; set; } = new List<ItemInventory>();
}
