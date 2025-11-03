using MediatR;
using Microsoft.Extensions.Logging;
using MobileBackend.Application.DTOs.Common;
using MobileBackend.Application.DTOs.Inventories;
using MobileBackend.Application.Interfaces;

namespace MobileBackend.Application.Features.Inventories.Queries.GetActiveInventories;

/// <summary>
/// Handler for getting active inventories
/// ? Optimized - Uses efficient eager loading
/// </summary>
public class GetActiveInventoriesQueryHandler : IRequestHandler<GetActiveInventoriesQuery, Result<List<InventoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetActiveInventoriesQueryHandler> _logger;

    public GetActiveInventoriesQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetActiveInventoriesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<InventoryDto>>> Handle(GetActiveInventoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // ? Uses efficient eager loading - single query with Include
            var inventories = await _unitOfWork.Inventories.GetActiveInventoriesAsync(cancellationToken);

            var inventoryDtos = inventories.Select(inv => new InventoryDto
            {
                Id = inv.Id,
                Name = inv.Name,
                Location = inv.Location,
                Description = inv.Description,
                IsActive = inv.IsActive,
                TotalItems = inv.ItemInventories?.Count ?? 0,  // ? From eager-loaded collection
                CreatedAt = inv.CreatedAt,
                UpdatedAt = inv.UpdatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} active inventories", inventoryDtos.Count);

            return Result<List<InventoryDto>>.SuccessResult(inventoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active inventories");
            return Result<List<InventoryDto>>.FailureResult("An error occurred while retrieving active inventories", 500);
        }
    }
}
