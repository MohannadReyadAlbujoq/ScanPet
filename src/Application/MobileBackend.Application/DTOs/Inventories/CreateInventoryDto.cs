namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for creating a new inventory/warehouse/section
/// </summary>
public class CreateInventoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional parent Location ID — makes this inventory a section within a Location
    /// </summary>
    public Guid? LocationId { get; set; }
}
