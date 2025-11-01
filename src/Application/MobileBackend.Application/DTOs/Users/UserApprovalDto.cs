namespace MobileBackend.Application.DTOs.Users;

/// <summary>
/// DTO for user approval operations
/// </summary>
public class UserApprovalDto
{
    public Guid UserId { get; set; }
    public bool IsApproved { get; set; }
    public bool IsEnabled { get; set; }
}
