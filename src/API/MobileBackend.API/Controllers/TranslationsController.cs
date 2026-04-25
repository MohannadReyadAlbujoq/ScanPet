using MediatR;
using Microsoft.AspNetCore.Mvc;
using MobileBackend.API.Controllers.Base;
using MobileBackend.Application.Common.Interfaces;

namespace MobileBackend.API.Controllers;

/// <summary>
/// v5 multilanguage support. Stores translated string values per
/// (entityType, entityId, languageCode, field) tuple.
///
/// POST /api/translations/{entityType}/{entityId}
///   body: [ { lang:"ar", name:"...", description:"..." }, { lang:"en", ... } ]
///
/// GET  /api/translations/{entityType}/{entityId}
///   Honors the Accept-Language header. "all" returns every language; missing/empty returns "en".
/// </summary>
[Route("api/[controller]")]
public class TranslationsController : BaseApiController
{
    private readonly ITranslationService _translations;
    private readonly ICurrentLanguage _currentLanguage;

    public TranslationsController(IMediator mediator,
                                  ILogger<TranslationsController> logger,
                                  ITranslationService translations,
                                  ICurrentLanguage currentLanguage)
        : base(mediator, logger)
    {
        _translations = translations;
        _currentLanguage = currentLanguage;
    }

    /// <summary>
    /// Replace ALL translations for the given entity. Each item in the array carries one language
    /// (key "lang") plus arbitrary string fields (e.g. "name", "description").
    /// </summary>
    [HttpPost("{entityType}/{entityId}")]
    public async Task<IActionResult> Set(string entityType, Guid entityId,
        [FromBody] List<Dictionary<string, object?>> body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return BadRequest(new { success = false, message = "entityType is required" });
        if (body == null || body.Count == 0)
            return BadRequest(new { success = false, message = "At least one language object is required" });

        var inputs = new List<TranslationInput>();
        foreach (var dict in body)
        {
            if (dict == null) continue;
            var lang = dict.FirstOrDefault(kv => string.Equals(kv.Key, "lang", StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(kv.Key, "languageCode", StringComparison.OrdinalIgnoreCase))
                           .Value?.ToString();
            if (string.IsNullOrWhiteSpace(lang)) continue;

            var fields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in dict)
            {
                if (string.Equals(kv.Key, "lang", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(kv.Key, "languageCode", StringComparison.OrdinalIgnoreCase)) continue;
                fields[kv.Key] = kv.Value?.ToString();
            }
            inputs.Add(new TranslationInput { LanguageCode = lang, Fields = fields });
        }

        await _translations.ReplaceTranslationsAsync(entityType, entityId, inputs, ct);
        return OkResponse("Translations saved");
    }

    /// <summary>
    /// Get the translations for the given entity. When Accept-Language is "all", returns every language.
    /// Otherwise returns the requested language (or "en" when missing).
    /// </summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<IActionResult> Get(string entityType, Guid entityId, CancellationToken ct)
    {
        var all = await _translations.GetTranslationsAsync(entityType, entityId, ct);

        if (_currentLanguage.AllRequested)
            return OkResponse(all);

        var code = string.IsNullOrWhiteSpace(_currentLanguage.Code) ? "en" : _currentLanguage.Code;
        if (all.TryGetValue(code, out var langGroup))
            return OkResponse(new { language = code, fields = langGroup });

        if (all.TryGetValue("en", out var en))
            return OkResponse(new { language = "en", fields = en });

        return OkResponse(new { language = code, fields = new Dictionary<string, string>() });
    }
}
