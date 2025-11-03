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
    public Guid InventoryId { get; set; }
    public string InventoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public string? Notes { get; set; }
    public bool IsLowStock { get; set; } // Calculated: Quantity <= MinimumQuantity
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
