using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Junction table for User's default inventories (many-to-many)
/// </summary>
public class UserDefaultInventory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid InventoryId { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Inventory Inventory { get; set; } = null!;
}
