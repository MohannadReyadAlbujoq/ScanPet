namespace MobileBackend.Application.DTOs.Auth;

/// <summary>
/// User registration request DTO
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// Unique username (3-50 characters)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password (min 8 characters, must meet complexity requirements)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Password confirmation (must match password)
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }
}
