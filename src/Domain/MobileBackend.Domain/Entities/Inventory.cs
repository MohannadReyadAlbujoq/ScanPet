using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Represents a warehouse or storage facility (inventory location / section)
/// Can optionally belong to a Location as a section within it
/// </summary>
public class Inventory : BaseEntity, ISoftDelete
{
    /// <summary>
    /// Name of the warehouse/inventory section
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Physical location/address of the warehouse (standalone description)
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
    
    /// <summary>
    /// Optional parent Location ID — makes this inventory a "section" within a Location
    /// </summary>
    public Guid? LocationId { get; set; }
    
    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    // Navigation Properties
    /// <summary>
    /// Items stored in this inventory with their quantities
    /// </summary>
    public virtual ICollection<ItemInventory> ItemInventories { get; set; } = new List<ItemInventory>();
    
    /// <summary>
    /// Parent location this inventory/section belongs to (optional)
    /// </summary>
    public virtual Location? ParentLocation { get; set; }
}
