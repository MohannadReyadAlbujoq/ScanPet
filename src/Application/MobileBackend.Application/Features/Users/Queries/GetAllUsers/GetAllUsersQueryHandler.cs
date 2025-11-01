using MediatR;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Users;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Handler for GetAllUsersQuery
/// Returns paginated list of users
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
        var (items, totalCount) = await _unitOfWork.Users.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate: null,
            orderBy: null,
            cancellationToken);

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
