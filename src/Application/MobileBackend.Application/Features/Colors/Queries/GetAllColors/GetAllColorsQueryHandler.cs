using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Colors.Queries.GetAllColors;

/// <summary>
/// Handler for getting all colors
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
            var colors = await _unitOfWork.Colors.GetAllAsync();

            var colorDtos = colors.Select(c => new ColorDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                RedValue = c.RedValue,
                GreenValue = c.GreenValue,
                BlueValue = c.BlueValue,
                HexCode = c.HexCode,
                ItemCount = c.Items?.Count ?? 0,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
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
