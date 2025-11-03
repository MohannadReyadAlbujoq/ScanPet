using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Colors.Queries.GetAllColors;

/// <summary>
/// Handler for getting all colors (with accurate item counts - no N+1)
/// </summary>
public class GetAllColorsQueryHandler : IRequestHandler<GetAllColorsQuery, Result<List<ColorDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllColorsQueryHandler> _logger;

    public GetAllColorsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllColorsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<ColorDto>>> Handle(GetAllColorsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Use optimized method with SQL aggregation ?
            var colorsWithCounts = await _unitOfWork.Colors.GetAllWithItemCountsAsync(cancellationToken);

            var colorDtos = colorsWithCounts.Select(tuple => new ColorDto
            {
                Id = tuple.Color.Id,
                Name = tuple.Color.Name,
                Description = tuple.Color.Description,
                RedValue = tuple.Color.RedValue,
                GreenValue = tuple.Color.GreenValue,
                BlueValue = tuple.Color.BlueValue,
                HexCode = tuple.Color.HexCode,
                ItemCount = tuple.ItemCount,  // ? Accurate count!
                CreatedAt = tuple.Color.CreatedAt,
                UpdatedAt = tuple.Color.UpdatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} colors", colorDtos.Count);

            return Result<List<ColorDto>>.SuccessResult(colorDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving colors");
            return Result<List<ColorDto>>.FailureResult("An error occurred while retrieving colors", 500);
        }
    }
}
