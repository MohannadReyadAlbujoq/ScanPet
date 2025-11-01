using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Commands.UpdateColor;

/// <summary>
/// Command to update an existing color
/// </summary>
public class UpdateColorCommand : IRequest<Result<bool>>
{
    public Guid ColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RedValue { get; set; }
    public int GreenValue { get; set; }
    public int BlueValue { get; set; }
}
