using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Colors.Queries.GetColorById;

/// <summary>
/// Handler for getting a color by ID
/// </summary>
public class GetColorByIdQueryHandler : IRequestHandler<GetColorByIdQuery, Result<ColorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetColorByIdQueryHandler> _logger;

    public GetColorByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetColorByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ColorDto>> Handle(GetColorByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var color = await _unitOfWork.Colors.GetByIdAsync(request.ColorId);
            
            if (color == null)
            {
                return Result<ColorDto>.FailureResult("Color not found", 404);
            }

            var colorDto = new ColorDto
            {
                Id = color.Id,
                Name = color.Name,
                Description = color.Description,
                RedValue = color.RedValue,
                GreenValue = color.GreenValue,
                BlueValue = color.BlueValue,
                HexCode = color.HexCode,
                ItemCount = color.Items?.Count ?? 0,
                CreatedAt = color.CreatedAt,
                UpdatedAt = color.UpdatedAt
            };

            _logger.LogInformation("Retrieved color: {ColorId} - {ColorName}", color.Id, color.Name);

            return Result<ColorDto>.SuccessResult(colorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving color: {ColorId}", request.ColorId);
            return Result<ColorDto>.FailureResult("An error occurred while retrieving the color", 500);
        }
    }
}
