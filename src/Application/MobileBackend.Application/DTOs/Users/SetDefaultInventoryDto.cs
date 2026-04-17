namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// DTO for setting user's default inventories
/// </summary>
public class SetDefaultInventoryDto
{
    /// <summary>
    /// Inventory IDs to set as default (empty list to clear all)
    /// </summary>
    public List<Guid> DefaultInventoryIds { get; set; } = new();
}
