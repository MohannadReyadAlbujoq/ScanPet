using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;

namespace MobileBackend.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Query to get user by ID
/// </summary>
public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}
