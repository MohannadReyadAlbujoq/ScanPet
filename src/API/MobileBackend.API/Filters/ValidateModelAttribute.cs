using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MobileBackend.API.Filters;

/// <summary>
/// Action filter that validates model state before action execution
/// Returns 400 Bad Request with validation errors if model state is invalid
/// Usage: [ValidateModel] (or apply globally)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Errors = e.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                })
                .ToList();

            var response = new
            {
                success = false,
                message = "Validation failed",
                errors = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }

        base.OnActionExecuting(context);
    }
}
