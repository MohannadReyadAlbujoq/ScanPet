using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Handler for GetAllUsersQuery
/// Returns paginated list of users with their roles (optimized - no N+1)
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
        // Use optimized method that includes roles in single query ?
        var (items, totalCount) = await _unitOfWork.Users.GetPagedWithRolesAsync(
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        // Map to DTOs - roles already loaded, no N+1! ?
        var userDtos = items.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsEnabled = user.IsEnabled,
            IsApproved = user.IsApproved,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        }).ToList();

        var pagedResult = PagedResult<UserDto>.Create(userDtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<UserDto>>.SuccessResult(pagedResult);
    }
}
