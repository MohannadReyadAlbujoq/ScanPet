using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;

namespace MobileBackend.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Query to get all users with pagination
/// </summary>
public class GetAllUsersQuery : IRequest<Result<PagedResult<UserDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
