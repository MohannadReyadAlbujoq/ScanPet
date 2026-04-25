namespace MobileBackend.Domain.Common;

/// <summary>
/// Marks a string property as searchable by the global keyword search.
/// Apply to string properties on entities (and on translation rows) you want
/// the <c>?keyword=</c> query param to match against.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SearchableAttribute : Attribute
{
}
