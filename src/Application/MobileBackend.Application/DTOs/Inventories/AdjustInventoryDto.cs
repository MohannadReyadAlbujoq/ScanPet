namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for adjusting inventory quantity (add or remove stock)
/// </summary>
public class AdjustInventoryDto
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int QuantityChange { get; set; } // Positive = add, Negative = remove
    public string? Notes { get; set; }
    public string? Reason { get; set; } // e.g., "Received shipment", "Damaged goods"
}
