namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// DTO for setting user's default inventory
/// </summary>
public class SetDefaultInventoryDto
{
    /// <summary>
    /// Inventory ID to set as default (null to clear)
    /// </summary>
    public Guid? DefaultInventoryId { get; set; }
}
