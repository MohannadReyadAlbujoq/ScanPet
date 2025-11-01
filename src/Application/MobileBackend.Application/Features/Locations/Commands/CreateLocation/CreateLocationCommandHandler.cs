using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.Common.Constants;
using MobileBackend.Application.Common.Interfaces;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.Interfaces;
using MobileBackend.Domain.Entities;

namespace MobileBackend.Application.Features.Locations.Commands.CreateLocation;

/// <summary>
/// Handler for creating a new location
/// </summary>
public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateLocationCommandHandler> _logger;

    public CreateLocationCommandHandler(
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUserService currentUserService,
        ILogger<CreateLocationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if location with same name already exists
            var existingLocation = await _unitOfWork.Locations.GetByNameAsync(request.Name, cancellationToken);
            if (existingLocation != null)
            {
                return Result<Guid>.FailureResult("A location with this name already exists", 409);
            }

            // Create new location
            var location = new Location
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                PostalCode = request.PostalCode,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Locations.AddAsync(location);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogAsync(
                action: AuditActions.LocationCreated,
                entityName: EntityNames.Location,
                entityId: location.Id,
                userId: _currentUserService.UserId ?? Guid.Empty,
                additionalInfo: $"Created location: {location.Name} in {location.City}, {location.Country}",
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("Location created successfully: {LocationId} - {LocationName}", location.Id, location.Name);

            return Result<Guid>.SuccessResult(location.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating location: {LocationName}", request.Name);
            return Result<Guid>.FailureResult("An error occurred while creating the location", 500);
        }
    }
}
