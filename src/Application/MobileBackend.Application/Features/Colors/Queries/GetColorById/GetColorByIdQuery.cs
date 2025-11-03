using MediatR;
using MobileBackend.Application.Common.Queries;
using MobileBackend.Application.DTOs.Colors;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Queries.GetColorById;

/// <summary>
/// Query to get a color by ID
/// </summary>
public class GetColorByIdQuery : BaseGetByIdQuery<ColorDto>
{
    // Backwards compatibility: Allow ColorId property
    public Guid ColorId 
    { 
        get => Id; 
        set => Id = value; 
    }
}
