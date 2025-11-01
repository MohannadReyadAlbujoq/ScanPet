using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Items.Commands.CreateItem;

/// <summary>
/// Handler for creating a new item
/// </summary>
public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateItemCommandHandler> _logger;

    public CreateItemCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<CreateItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate ColorId if provided
            if (request.ColorId.HasValue)
            {
                var color = await _unitOfWork.Colors.GetByIdAsync(request.ColorId.Value);
                if (color == null)
                {
                    return Result<Guid>.FailureResult("Color not found", 404);
                }
            }

            // Create new item
            var item = new Item
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                SKU = request.SKU,
                BasePrice = request.BasePrice,
                Quantity = request.Quantity,
                ColorId = request.ColorId,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Items.AddAsync(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.ItemCreated,
                entityName: EntityNames.Item,
                entityId: item.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Created item: {item.Name}, Price: {item.BasePrice}, Quantity: {item.Quantity}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Item created successfully: {ItemId} - {ItemName}", item.Id, item.Name);

            return Result<Guid>.SuccessResult(item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item: {ItemName}", request.Name);
            return Result<Guid>.FailureResult("An error occurred while creating the item", 500);
        }
    }
}
