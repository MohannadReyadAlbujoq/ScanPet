using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Queries.GetAllColors;

/// <summary>
/// Query to get all colors with optional pagination
/// </summary>
public class GetAllColorsQuery : BasePagedQuery<ColorDto>
{
}
