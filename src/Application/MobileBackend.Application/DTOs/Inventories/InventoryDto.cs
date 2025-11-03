namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for Inventory (Warehouse) information
/// </summary>
public class InventoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TotalItems { get; set; } // Count of different items stored
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
