using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Colors.Commands.CreateColor;

/// <summary>
/// Handler for creating a new color
/// </summary>
public class CreateColorCommandHandler : IRequestHandler<CreateColorCommand, Result<Guid>>
{
    private readonly IColorRepository _colorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateColorCommandHandler> _logger;

    public CreateColorCommandHandler(
        IColorRepository colorRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<CreateColorCommandHandler> logger)
    {
        _colorRepository = colorRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateColorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if color with same name already exists
            var existingColor = await _colorRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingColor != null)
            {
                return Result<Guid>.FailureResult("A color with this name already exists", 409);
            }

            // Create new color
            var color = new Color
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                RedValue = request.RedValue,
                GreenValue = request.GreenValue,
                BlueValue = request.BlueValue,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _colorRepository.AddAsync(color);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.ColorCreated,
                entityName: EntityNames.Color,
                entityId: color.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Created color: {color.Name} (RGB: {color.RedValue}, {color.GreenValue}, {color.BlueValue})",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Color created successfully: {ColorId} - {ColorName}", color.Id, color.Name);

            return Result<Guid>.SuccessResult(color.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating color: {ColorName}", request.Name);
            return Result<Guid>.FailureResult("An error occurred while creating the color", 500);
        }
    }
}
