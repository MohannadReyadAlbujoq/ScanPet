using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.ApproveUser;

/// <summary>
/// Command to approve/enable a user account
/// </summary>
public class ApproveUserCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
    public bool IsApproved { get; set; }
    public bool IsEnabled { get; set; }
}
