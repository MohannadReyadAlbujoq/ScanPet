namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for setting up initial inventory for an item
/// </summary>
public class SetItemInventoryDto
{
    public Guid ItemId { get; set; }
    public Guid InventoryId { get; set; }
    public int Quantity { get; set; }
    public int? MinimumQuantity { get; set; }
    public int? MaximumQuantity { get; set; }
    public string? Notes { get; set; }
}
