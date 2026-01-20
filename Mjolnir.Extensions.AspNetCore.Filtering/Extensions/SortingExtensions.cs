using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

public static class SortingExtensions
{
    private static TOrdered SplitAndApply<TSource, TOrdered, TItem>(
        this TSource source,
        string sortString,
        Func<TSource, Expression<Func<TItem, object>>, SortDirection, TOrdered> sortFunc,
        Func<TOrdered, Expression<Func<TItem, object>>, SortDirection, TOrdered> thenFunc
    )
    {
        StringBuilder sb = new();
        foreach (char c in sortString.Where(c => !char.IsWhiteSpace(c))) sb.Append(c);
        string[] sorting = sb.ToString().Split(',');

        if (sorting.Length == 0) throw new ArgumentException("No sorting specified");

        bool first = true;
        TOrdered? ordered = default;
        foreach (string sort in sorting)
        {
            string[] parts = sort.Split(':');
            string propertyName = parts[0];
            string value = parts[1];
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            PropertyInfo prop = typeof(TItem).GetProperty(propertyName) ??
                                throw new ArgumentException($"Property {propertyName} not found");

            Expression<Func<TItem, object>> keySelector =
                prop.GetCustomAttribute<SortableAttribute>()
                    ?.BuildKeySelector<TItem>(prop, propertyName, value, out SortDirection dir) ??
                prop.GetCustomAttribute<CustomSortingAttribute<TItem>>()?.KeySelector(prop, propertyName, value, out dir) ??
                throw new ArgumentException($"No sorting attribute found for property {propertyName}");

            ordered = first ? sortFunc(source, keySelector, dir) : thenFunc(ordered!, keySelector, dir);
            first = false;
        }

        return ordered!;
    }

    public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> source, string sortString) => source
        .SplitAndApply<IQueryable<T>, IOrderedQueryable<T>, T>(
            sortString,
            (src, selector, dir) => dir is SortDirection.Asc ? src.OrderBy(selector) : src.OrderByDescending(selector),
            (src, selector, dir) => dir is SortDirection.Asc ? src.ThenBy(selector) : src.ThenByDescending(selector)
        );

    public static IOrderedEnumerable<T> SortBy<T>(this IEnumerable<T> source, string sortString) => source
        .SplitAndApply<IEnumerable<T>, IOrderedEnumerable<T>, T>(
            sortString,
            (src, selector, dir) => dir is SortDirection.Asc ? src.OrderBy(selector.Compile()) : src.OrderByDescending(selector.Compile()),
            (src, selector, dir) => dir is SortDirection.Asc ? src.ThenBy(selector.Compile()) : src.ThenByDescending(selector.Compile())
        );
}
