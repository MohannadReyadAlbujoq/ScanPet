using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Users;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Users.Queries.SearchUsers;

/// <summary>
/// Handler for searching users by username, email, or full name
/// Uses BaseSearchHandler to eliminate code duplication
/// </summary>
public class SearchUsersQueryHandler : BaseSearchHandler<SearchUsersQuery, User, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public SearchUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<SearchUsersQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<List<User>> GetAllEntitiesAsync(CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.ToList();
    }

    protected override bool MatchesSearchTerm(User entity, string searchTerm)
    {
        return entity.Username.ToLower().Contains(searchTerm) ||
               entity.Email.ToLower().Contains(searchTerm) ||
               (entity.FullName != null && entity.FullName.ToLower().Contains(searchTerm)) ||
               (entity.PhoneNumber != null && entity.PhoneNumber.Contains(searchTerm));
    }

    protected override UserDto MapToDto(User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            FullName = entity.FullName,
            PhoneNumber = entity.PhoneNumber,
            IsEnabled = entity.IsEnabled,
            IsApproved = entity.IsApproved,
            Roles = entity.UserRoles?.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.User;
}
