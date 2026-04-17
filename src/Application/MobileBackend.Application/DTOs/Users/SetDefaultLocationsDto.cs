namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// DTO for setting user's default locations
/// </summary>
public class SetDefaultLocationsDto
{
    /// <summary>
    /// Location IDs to set as default (empty list to clear all)
    /// </summary>
    public List<Guid> DefaultLocationIds { get; set; } = new();
}
