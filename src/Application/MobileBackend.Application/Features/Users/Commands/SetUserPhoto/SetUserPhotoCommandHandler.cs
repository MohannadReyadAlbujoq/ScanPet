using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Users.Commands.SetUserPhoto;

public class SetUserPhotoCommandHandler : IRequestHandler<SetUserPhotoCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetUserPhotoCommandHandler> _logger;

    public SetUserPhotoCommandHandler(
        IUnitOfWork unitOfWork,
        IFileService fileService,
        ICurrentUserService currentUserService,
        ILogger<SetUserPhotoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SetUserPhotoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.UserId == Guid.Empty)
                return Result<string>.FailureResult("UserId is required", 400);

            if (request.FileStream == Stream.Null || request.FileStream == null)
                return Result<string>.FailureResult("Photo file is required", 400);

            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result<string>.FailureResult("User not found", 404);

            // Delete old photo (best-effort)
            if (!string.IsNullOrEmpty(user.PhotoUrl))
            {
                try { _fileService.DeleteFile(user.PhotoUrl); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old user photo {Url}", user.PhotoUrl); }
            }

            var url = await _fileService.SaveFileAsync(request.FileStream, request.FileName, "users", cancellationToken);
            user.PhotoUrl = url;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = _currentUserService.UserId;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting photo for user {UserId}", request.UserId);
            return Result<string>.FailureResult("An error occurred while uploading the photo", 500);
        }
    }
}
