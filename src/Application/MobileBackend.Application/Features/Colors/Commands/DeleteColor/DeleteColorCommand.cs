using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Colors.Commands.DeleteColor;

/// <summary>
/// Command to delete (soft delete) a color
/// </summary>
public class DeleteColorCommand : IRequest<Result<bool>>
{
    public Guid ColorId { get; set; }
}
