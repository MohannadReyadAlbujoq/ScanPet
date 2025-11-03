namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for updating an inventory/warehouse
/// </summary>
public class UpdateInventoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
