namespace MobileBackend.Application.DTOs.Inventories;

/// <summary>
/// DTO for creating a new inventory/warehouse
/// </summary>
public class CreateInventoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
