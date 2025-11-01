using MediatR;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Queries.GetAllColors;

/// <summary>
/// Query to get all colors
/// </summary>
public class GetAllColorsQuery : IRequest<Result<List<ColorDto>>>
{
}
