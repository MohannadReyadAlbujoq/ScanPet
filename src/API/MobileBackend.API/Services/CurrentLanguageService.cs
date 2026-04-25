using Microsoft.AspNetCore.Http;
using MobileBackend.Application.Common.Interfaces;

namespace MobileBackend.API.Services;

/// <summary>
/// Reads <c>Accept-Language</c> (falls back to <c>X-Language</c>) from the current HTTP request.
/// Defaults to "en" when missing/empty.
/// </summary>
public class CurrentLanguageService : ICurrentLanguage
{
    public CurrentLanguageService(IHttpContextAccessor accessor)
    {
        var ctx = accessor.HttpContext;
        var raw = (ctx?.Request.Headers["Accept-Language"].ToString())
                  ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            raw = ctx?.Request.Headers["X-Language"].ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            Code = "en";
            return;
        }

        // Pick the first token, lower-case, drop region (e.g. "ar-JO" -> "ar")
        var first = raw.Split(',')[0].Trim().Split('-')[0].ToLowerInvariant();
        Code = string.IsNullOrEmpty(first) ? "en" : first;
    }

    public string Code { get; }
    public bool AllRequested => string.Equals(Code, "all", StringComparison.OrdinalIgnoreCase);
}
