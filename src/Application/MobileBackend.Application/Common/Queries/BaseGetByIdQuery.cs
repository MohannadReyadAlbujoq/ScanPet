using MediatR;
using MobileBackend.Application.DTOs.Common;

namespace MobileBackend.Application.Common.Queries;

/// <summary>
/// Base query for get by ID operations
/// Eliminates duplication across all GetById queries
/// </summary>
/// <typeparam name="TDto">The DTO type to return</typeparam>
public abstract class BaseGetByIdQuery<TDto> : IRequest<Result<TDto>>
{
    /// <summary>
    /// Entity ID to retrieve
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Validates the query
    /// </summary>
    public virtual bool IsValid(out string? errorMessage)
    {
        if (Id == Guid.Empty)
        {
            errorMessage = "ID cannot be empty";
            return false;
        }

        errorMessage = null;
        return true;
    }
}
