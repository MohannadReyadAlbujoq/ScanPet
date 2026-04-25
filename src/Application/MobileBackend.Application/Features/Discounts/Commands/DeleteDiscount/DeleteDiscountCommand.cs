using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
