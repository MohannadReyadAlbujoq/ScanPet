using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Roles;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Roles.Queries.SearchRoles;

/// <summary>
/// Handler for searching roles by name or description
/// Uses BaseSearchHandler to eliminate code duplication
/// </summary>
public class SearchRolesQueryHandler : BaseSearchHandler<SearchRolesQuery, Role, RoleDto>
{
    private readonly IRoleRepository _roleRepository;

    public SearchRolesQueryHandler(
        IRoleRepository roleRepository,
        ILogger<SearchRolesQueryHandler> logger)
        : base(logger)
    {
        _roleRepository = roleRepository;
    }

    protected override async Task<List<Role>> GetAllEntitiesAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return roles.ToList();
    }

    protected override bool MatchesSearchTerm(Role entity, string searchTerm)
    {
        return entity.Name.ToLower().Contains(searchTerm) ||
               (entity.Description != null && entity.Description.ToLower().Contains(searchTerm));
    }

    protected override RoleDto MapToDto(Role entity)
    {
        return new RoleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Role;
}
