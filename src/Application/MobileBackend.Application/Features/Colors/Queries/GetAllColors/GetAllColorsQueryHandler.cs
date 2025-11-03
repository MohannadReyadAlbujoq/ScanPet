using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Queries.GetAllColors;

/// <summary>
/// Handler for getting all colors (with accurate item counts - no N+1)
/// Uses BaseGetAllHandler pattern while maintaining optimized SQL aggregation
/// </summary>
public class GetAllColorsQueryHandler : BaseGetAllHandler<GetAllColorsQuery, Color, ColorDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private Dictionary<Guid, int> _itemCounts = new();

    public GetAllColorsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllColorsQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<List<Color>> GetEntitiesAsync(GetAllColorsQuery request, CancellationToken cancellationToken)
    {
        // Use optimized method with SQL aggregation ?
        var colorsWithCounts = await _unitOfWork.Colors.GetAllWithItemCountsAsync(cancellationToken);
        
        // Store item counts in instance variable for use in MapToDto
        _itemCounts = colorsWithCounts.ToDictionary(t => t.Color.Id, t => t.ItemCount);
        
        // Return just the colors
        return colorsWithCounts.Select(t => t.Color).ToList();
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
            HexCode = entity.HexCode,
            ItemCount = _itemCounts.TryGetValue(entity.Id, out var count) ? count : 0, // ? Accurate count from SQL!
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Color;
}
