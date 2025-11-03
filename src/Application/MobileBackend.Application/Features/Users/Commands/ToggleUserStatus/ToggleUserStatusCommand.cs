using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.ToggleUserStatus;

/// <summary>
/// Command to enable or disable a user
/// </summary>
public class ToggleUserStatusCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; }
}
