using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Handler for GetAllUsersQuery
/// Returns paginated list of users with their roles
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // Get paginated users
        var (items, totalCount) = await _unitOfWork.Users.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate: null,
            orderBy: null,
            cancellationToken);

        // Map to DTOs with roles
        var userDtos = new List<UserDto>();
        foreach (var user in items)
        {
            // Fetch roles for each user
            var roles = await _unitOfWork.Roles.GetRolesByUserIdAsync(user.Id, cancellationToken);
            
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsEnabled = user.IsEnabled,
                IsApproved = user.IsApproved,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        var pagedResult = PagedResult<UserDto>.Create(userDtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<UserDto>>.SuccessResult(pagedResult);
    }
}
