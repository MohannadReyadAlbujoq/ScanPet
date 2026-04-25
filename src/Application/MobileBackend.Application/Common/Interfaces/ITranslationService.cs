namespace MobileBackend.Application.Common.Interfaces;

/// <summary>
/// One language group submitted in a create/update body.
/// </summary>
public class TranslationInput
{
    public string LanguageCode { get; set; } = string.Empty;
    public Dictionary<string, string?> Fields { get; set; } = new();
}

/// <summary>
/// CRUD over the generic <c>EntityTranslations</c> table. Used by every
/// multilanguage-aware entity (Item, Color, Location, Inventory, Role).
/// </summary>
public interface ITranslationService
{
    Task ReplaceTranslationsAsync(string entityType, Guid entityId,
        IEnumerable<TranslationInput> translations, CancellationToken ct = default);

    /// <summary>Returns all translations grouped by language code.</summary>
    Task<Dictionary<string, Dictionary<string, string>>> GetTranslationsAsync(
        string entityType, Guid entityId, CancellationToken ct = default);
}
