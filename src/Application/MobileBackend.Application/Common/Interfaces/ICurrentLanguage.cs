namespace MobileBackend.Application.Common.Interfaces;

/// <summary>
/// Resolves the active language for the current request from the
/// <c>Accept-Language</c> (or <c>X-Language</c>) header. Defaults to "en"
/// when the header is missing or empty. The special value "all" instructs
/// handlers to return every available translation.
/// </summary>
public interface ICurrentLanguage
{
    /// <summary>Lower-case 2-letter code, or "all".</summary>
    string Code { get; }

    /// <summary>True when the caller asked for every translation.</summary>
    bool AllRequested { get; }
}
