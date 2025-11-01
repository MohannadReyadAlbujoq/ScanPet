using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;

namespace MobileBackend.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user (admin only)
/// </summary>
public class CreateUserCommand : IRequest<Result<Guid>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
