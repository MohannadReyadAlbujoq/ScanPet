namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// Request DTO for toggling user status (enable/disable)
/// </summary>
public class ToggleUserStatusDto
{
    /// <summary>
    /// True to enable the user, false to disable
    /// </summary>
    public bool IsEnabled { get; set; }
}
