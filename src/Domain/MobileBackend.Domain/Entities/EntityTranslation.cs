using MobileBackend.Domain.Common;

namespace MobileBackend.Domain.Entities;

/// <summary>
/// Generic translation row. Stores one translated value for one (EntityType, EntityId, LanguageCode, Field) tuple.
/// EntityType uses the simple .NET type name (e.g. "Item", "Color", "Location", "Inventory", "Role").
/// Field is the name of the property being translated (e.g. "Name", "Description", "Address").
/// </summary>
public class EntityTranslation : BaseEntity, ISoftDelete
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }

    /// <summary>ISO 639-1 lower-case (e.g. "en", "ar").</summary>
    public string LanguageCode { get; set; } = string.Empty;

    public string Field { get; set; } = string.Empty;

    /// <summary>Marked Searchable so the global keyword search hits ALL languages, not just the current one.</summary>
    [Searchable]
    public string Value { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
