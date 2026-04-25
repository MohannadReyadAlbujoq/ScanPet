namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// Top-level response for GET /api/inventories/item-counts.
/// Contains global totals across every inventory + per-inventory breakdown.
/// </summary>
public class AllInventoriesItemCountDto
{
    /// <summary>Total distinct item types that exist across all inventories (de-duplicated)</summary>
    public int GlobalTotalItemTypes { get; set; }

    /// <summary>Sum of every item quantity across every inventory</summary>
    public int GlobalTotalQuantity { get; set; }

    /// <summary>Global view: each unique item with its combined quantity from all inventories</summary>
    public List<GlobalItemTotalDto> GlobalItemTotals { get; set; } = new();

    /// <summary>Per-inventory breakdown (same shape as the single-inventory endpoint)</summary>
    public List<InventoryItemCountDto> Inventories { get; set; } = new();
}

/// <summary>
/// One row in the global totals list — a single item aggregated across all inventories.
/// </summary>
public class GlobalItemTotalDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemSKU { get; set; }
    public string? ItemImageUrl { get; set; }

    /// <summary>Combined quantity of this item across every inventory</summary>
    public int TotalQuantity { get; set; }

    /// <summary>How many inventories stock this item</summary>
    public int InventoryCount { get; set; }

    /// <summary>True when the item is low-stock in at least one inventory</summary>
    public bool IsLowStockAnywhere { get; set; }
}

/// <summary>
/// Summary of item quantities for a single inventory/warehouse.
/// Returned by GET /api/inventories/{id}/item-counts
/// and embedded inside AllInventoriesItemCountDto.Inventories.
/// </summary>
public class InventoryItemCountDto
{
    public Guid InventoryId { get; set; }
    public string InventoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>Number of distinct item types stored in this inventory</summary>
    public int TotalItemTypes { get; set; }

    /// <summary>Total quantity of all items combined in this inventory</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Per-item breakdown</summary>
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
    public string? ItemImageUrl { get; set; }
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public bool IsLowStock { get; set; }
}
