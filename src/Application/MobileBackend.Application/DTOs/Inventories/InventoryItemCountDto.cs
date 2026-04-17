namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// Summary of item quantities for a single inventory/warehouse
/// </summary>
public class InventoryItemCountDto
{
    public Guid InventoryId { get; set; }
    public string InventoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of distinct item types stored in this inventory
    /// </summary>
    public int TotalItemTypes { get; set; }

    /// <summary>
    /// Total quantity of all items combined
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// Per-item breakdown
    /// </summary>
    public List<ItemCountEntryDto> Items { get; set; } = new();
}

/// <summary>
/// Single item entry in the inventory count summary
/// </summary>
public class ItemCountEntryDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemSKU { get; set; }
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public bool IsLowStock { get; set; }
}
