using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using MobileBackend.Domain.Common;

namespace MobileBackend.Application.Common.Search;

/// <summary>
/// Global keyword search helper. Walks every property on an entity that is
/// marked with <see cref="SearchableAttribute"/> ? plus every entry in any
/// IEnumerable property whose element type also has searchable properties
/// (used to search across translation rows in ALL languages).
/// </summary>
public static class KeywordSearchExtensions
{
    private static readonly ConcurrentDictionary<Type, SearchPlan> _plans = new();

    private sealed class SearchPlan
    {
        public List<PropertyInfo> StringProps { get; } = new();
        public List<(PropertyInfo Collection, SearchPlan ItemPlan)> CollectionProps { get; } = new();
    }

    private static SearchPlan GetPlan(Type t) => _plans.GetOrAdd(t, BuildPlan);

    private static SearchPlan BuildPlan(Type t)
    {
        var plan = new SearchPlan();
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetCustomAttribute<SearchableAttribute>() != null && p.PropertyType == typeof(string))
            {
                plan.StringProps.Add(p);
            }
            else if (p.PropertyType != typeof(string)
                     && typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
            {
                var itemType = p.PropertyType.IsGenericType
                    ? p.PropertyType.GetGenericArguments().FirstOrDefault()
                    : null;
                if (itemType != null && itemType != typeof(string))
                {
                    var itemPlan = GetPlan(itemType);
                    if (itemPlan.StringProps.Count > 0 || itemPlan.CollectionProps.Count > 0)
                        plan.CollectionProps.Add((p, itemPlan));
                }
            }
        }
        return plan;
    }

    public static IEnumerable<T> ApplyKeyword<T>(this IEnumerable<T> source, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return source;
        var k = keyword.Trim();
        var plan = GetPlan(typeof(T));
        if (plan.StringProps.Count == 0 && plan.CollectionProps.Count == 0) return source;
        return source.Where(x => x != null && Matches(x, plan, k));
    }

    private static bool Matches(object obj, SearchPlan plan, string keyword)
    {
        foreach (var p in plan.StringProps)
        {
            if (p.GetValue(obj) is string s && s.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        foreach (var (collProp, itemPlan) in plan.CollectionProps)
        {
            if (collProp.GetValue(obj) is IEnumerable list)
            {
                foreach (var item in list)
                {
                    if (item != null && Matches(item, itemPlan, keyword))
                        return true;
                }
            }
        }
        return false;
    }
}
