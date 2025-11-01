using MediatR;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Queries.GetColorById;

/// <summary>
/// Query to get a color by ID
/// </summary>
public class GetColorByIdQuery : IRequest<Result<ColorDto>>
{
    public Guid ColorId { get; set; }
}
