using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Discounts.Commands.DeleteDiscount;

public class DeleteDiscountCommandHandler : IRequestHandler<DeleteDiscountCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteDiscountCommandHandler> _logger;

    public DeleteDiscountCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<DeleteDiscountCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var d = await _unitOfWork.Discounts.GetByIdAsync(request.Id, cancellationToken);
            if (d == null)
                return Result<bool>.FailureResult("Discount not found", 404);
            d.IsDeleted = true;
            d.DeletedAt = DateTime.UtcNow;
            d.DeletedBy = _currentUserService.UserId;
            _unitOfWork.Discounts.Update(d);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting discount {Id}", request.Id);
            return Result<bool>.FailureResult("An error occurred while deleting the discount", 500);
        }
    }
}
