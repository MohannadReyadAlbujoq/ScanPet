using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Commands.DeleteUserPhoto;

public class DeleteUserPhotoCommandHandler : IRequestHandler<DeleteUserPhotoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteUserPhotoCommandHandler> _logger;

    public DeleteUserPhotoCommandHandler(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        ICurrentUserService currentUserService,
        ILogger<DeleteUserPhotoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteUserPhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result<bool>.FailureResult("User not found", 404);

            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                try { _fileService.DeleteFile(user.PhotoUrl); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete user photo file {Url}", user.PhotoUrl); }
            }

            user.PhotoUrl = null;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo for user {UserId}", request.UserId);
            return Result<bool>.FailureResult("An error occurred while removing the photo", 500);
        }
    }
}
