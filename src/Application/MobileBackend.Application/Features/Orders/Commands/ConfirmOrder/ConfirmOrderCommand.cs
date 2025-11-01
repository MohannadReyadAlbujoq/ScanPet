using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Orders.Commands.ConfirmOrder;

/// <summary>
/// Command to confirm a pending order
/// </summary>
public class ConfirmOrderCommand : IRequest<Result<bool>>
{
    public Guid OrderId { get; set; }
}
