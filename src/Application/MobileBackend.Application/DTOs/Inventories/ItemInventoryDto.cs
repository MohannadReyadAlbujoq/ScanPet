namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for Item Inventory (item quantity at specific warehouse)
/// </summary>
public class ItemInventoryDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemSKU { get; set; }
    public string? ItemImageUrl { get; set; }
    public decimal? ItemBasePrice { get; set; }
    public string? ColorName { get; set; }
    public string? ColorHexCode { get; set; }
    public Guid InventoryId { get; set; }
    public string InventoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public string? Notes { get; set; }
    public bool IsLowStock { get; set; } // Calculated: Quantity <= MinimumQuantity

    /// <summary>All discounts that target this item-at-this-inventory (Item / ItemInventory / ItemLocation).</summary>
    public List<MobileBackend.Application.DTOs.Discounts.DiscountDto>? Discounts { get; set; }

    /// <summary>Effective per-unit discount after stack/exclusive resolution. Null when none.</summary>
    public decimal? EffectivePerUnitDiscount { get; set; }

    /// <summary>Effective per-unit price = Item.BasePrice - EffectivePerUnitDiscount (clamped at 0).</summary>
    public decimal? EffectiveUnitPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
