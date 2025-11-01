using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Commands.CreateColor;

/// <summary>
/// Command to create a new color
/// </summary>
public class CreateColorCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RedValue { get; set; }
    public int GreenValue { get; set; }
    public int BlueValue { get; set; }
}
