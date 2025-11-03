using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Users;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery
/// Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetUserByIdQueryHandler : BaseGetByIdHandler<GetUserByIdQuery, User, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUserByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<User?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Users.GetByIdWithRolesAsync(id, cancellationToken);
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
            Roles = entity.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.User;
}
