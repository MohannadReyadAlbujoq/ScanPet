namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for updating item quantity at a specific inventory (absolute set)
/// Used by PUT /api/inventories/items
/// </summary>
public class UpdateItemQuantityDto
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public string? Notes { get; set; }
}
