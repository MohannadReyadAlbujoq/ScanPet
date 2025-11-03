namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for transferring inventory between warehouses
/// </summary>
public class TransferInventoryDto
{
    public Guid ItemId { get; set; }
    public Guid FromInventoryId { get; set; }
    public Guid ToInventoryId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public string? Reason { get; set; } // e.g., "Rebalancing stock", "Customer request"
}
