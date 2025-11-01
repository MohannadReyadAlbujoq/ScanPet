namespace MobileBackend.Application.DTOs.Auth;

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// Current password for verification
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password (must meet complexity requirements)
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password confirmation (must match new password)
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
