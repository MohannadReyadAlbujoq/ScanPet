using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Queries.SearchColors;

/// <summary>
/// Handler for searching colors by name or description
/// Uses BaseSearchHandler to eliminate code duplication
/// </summary>
public class SearchColorsQueryHandler : BaseSearchHandler<SearchColorsQuery, Color, ColorDto>
{
    private readonly IColorRepository _colorRepository;

    public SearchColorsQueryHandler(
        IColorRepository colorRepository,
        ILogger<SearchColorsQueryHandler> logger)
        : base(logger)
    {
        _colorRepository = colorRepository;
    }

    protected override async Task<List<Color>> GetAllEntitiesAsync(CancellationToken cancellationToken)
    {
        var colors = await _colorRepository.GetAllAsync(cancellationToken);
        return colors.ToList();
    }

    protected override bool MatchesSearchTerm(Color entity, string searchTerm)
    {
        return entity.Name.ToLower().Contains(searchTerm) ||
               (entity.Description != null && entity.Description.ToLower().Contains(searchTerm));
    }

    protected override ColorDto MapToDto(Color entity)
    {
        return new ColorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            RedValue = entity.RedValue,
            GreenValue = entity.GreenValue,
            BlueValue = entity.BlueValue,
            HexCode = $"#{entity.RedValue:X2}{entity.GreenValue:X2}{entity.BlueValue:X2}",
            ItemCount = entity.Items?.Count(i => !i.IsDeleted) ?? 0,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Color;
}
