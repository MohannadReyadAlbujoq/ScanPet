using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Handlers;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Queries.GetColorById;

/// <summary>
/// Handler for getting a color by ID (with accurate item count)
/// Uses BaseGetByIdHandler to eliminate code duplication
/// </summary>
public class GetColorByIdQueryHandler : BaseGetByIdHandler<GetColorByIdQuery, Color, ColorDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private int _itemCount = 0;

    public GetColorByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetColorByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<Color?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // Use optimized method with SQL aggregation ?
        var (color, itemCount) = await _unitOfWork.Colors.GetByIdWithItemCountAsync(id, cancellationToken);
        
        // Store item count for use in MapToDto
        _itemCount = itemCount;
        
        return color;
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
            ItemCount = _itemCount,  // ? Accurate count from SQL!
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    protected override string GetEntityName() => EntityNames.Color;
}
