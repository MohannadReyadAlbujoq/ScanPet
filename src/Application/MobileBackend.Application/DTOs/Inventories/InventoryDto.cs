namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for Inventory (Warehouse/Section) information
/// </summary>
public class InventoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TotalItems { get; set; }
    
    /// <summary>
    /// Parent Location ID (null if standalone inventory)
    /// </summary>
    public Guid? LocationId { get; set; }
    
    /// <summary>
    /// Parent Location name (only in responses)
    /// </summary>
    public string? LocationName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
