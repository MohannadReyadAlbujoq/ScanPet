using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.DeleteUserPhoto;

public class DeleteUserPhotoCommand : IRequest<Result<bool>>
{
    public Guid UserId { get; set; }
}
