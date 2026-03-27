namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for incrementing item quantity at a specific inventory
/// Used by POST /api/inventories/increment
/// </summary>
public class IncrementInventoryDto
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
    public string? Reason { get; set; }
}
