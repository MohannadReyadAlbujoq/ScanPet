using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;

namespace MobileBackend.Application.Features.Users.Queries.SearchUsers;

/// <summary>
/// Query to search users by username, email, or full name
/// Inherits pagination and search term from BaseSearchQuery
/// </summary>
public class SearchUsersQuery : BaseSearchQuery<UserDto>, IRequest<Result<List<UserDto>>>
{
}
