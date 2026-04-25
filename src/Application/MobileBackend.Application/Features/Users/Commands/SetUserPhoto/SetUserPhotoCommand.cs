using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Users.Commands.SetUserPhoto;

/// <summary>
/// Command to set/replace a user's profile photo. Returns the new PhotoUrl.
/// </summary>
public class SetUserPhotoCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
}
