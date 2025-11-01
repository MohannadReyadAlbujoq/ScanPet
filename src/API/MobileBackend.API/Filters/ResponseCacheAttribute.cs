using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace MobileBackend.API.Filters;

/// <summary>
/// Caches GET request responses to improve performance
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ResponseCacheAttribute : ActionFilterAttribute
{
    private readonly int _durationSeconds;
    private readonly bool _varyByUser;
    
    public ResponseCacheAttribute(int durationSeconds = 60, bool varyByUser = false)
    {
        _durationSeconds = durationSeconds;
        _varyByUser = varyByUser;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Only cache GET requests
        if (context.HttpContext.Request.Method != HttpMethods.Get)
        {
            await next();
            return;
        }

        var cache = context.HttpContext.RequestServices
            .GetRequiredService<IMemoryCache>();

        var cacheKey = GenerateCacheKey(context);

        // Try to get from cache
        if (cache.TryGetValue(cacheKey, out object? cachedResponse))
        {
            context.Result = new ObjectResult(cachedResponse)
            {
                StatusCode = 200
            };
            return;
        }

        // Execute action and cache result
        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult &&
            objectResult.StatusCode == 200)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_durationSeconds),
                Priority = CacheItemPriority.Normal
            };

            cache.Set(cacheKey, objectResult.Value, cacheOptions);
        }
    }

    private string GenerateCacheKey(ActionExecutingContext context)
    {
        var keyBuilder = new StringBuilder();
        
        // Add route
        keyBuilder.Append(context.HttpContext.Request.Path);
        
        // Add query string
        if (context.HttpContext.Request.QueryString.HasValue)
        {
            keyBuilder.Append(context.HttpContext.Request.QueryString.Value);
        }
        
        // Add user ID if varying by user
        if (_varyByUser)
        {
            var userId = context.HttpContext.User?.FindFirst("sub")?.Value ?? "anonymous";
            keyBuilder.Append($"_user_{userId}");
        }

        // Generate hash for shorter key
        var key = keyBuilder.ToString();
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(hash);
    }
}
