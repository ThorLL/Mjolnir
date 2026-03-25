using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

/// <summary>
///     Extension methods for sorting sequences of entities using sort strings.
///     Supports both <see cref="IQueryable{T}" /> and <see cref="IEnumerable{T}" /> sources.
/// </summary>
public static class SortingExtensions
{
    /// <summary>
    ///     Applies sorting to a sequence by parsing a sort string and applying matching attribute key selectors.
    ///     Sort string format: "property1:asc|desc,property2:asc|desc,..."
    /// </summary>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TOrdered">The return type after ordering (e.g., <see cref="IOrderedQueryable{T}" /> or <see cref="IOrderedEnumerable{T}" />).</typeparam>
    /// <typeparam name="TItem">The type of items in the source sequence.</typeparam>
    /// <param name="source">The source sequence to sort.</param>
    /// <param name="sortString">The sort specification string.</param>
    /// <param name="sortFunc">A function that applies the first sort operation to the source sequence.</param>
    /// <param name="thenFunc">A function that applies subsequent sort operations to an already-sorted sequence.</param>
    /// <returns>The sorted sequence with all sort conditions applied in order.</returns>
    /// <exception cref="MjolnirException">Thrown if a property is not found, has no sort attribute, or if no sort is specified.</exception>
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

        MjolnirException.ThrowIfZero(sorting.Length, "No sorting specified");

        bool first = true;
        TOrdered? ordered = default;
        foreach (string sort in sorting)
        {
            string[] parts = sort.Split(':');
            string propertyName = parts[0];
            string value = parts[1];
            MjolnirException.ThrowIfNullOrWhiteSpace(propertyName);
            MjolnirException.ThrowIfNullOrWhiteSpace(value);

            PropertyInfo? prop = typeof(TItem).GetProperty(propertyName);
            MjolnirException.ThrowIfNull(prop, $"Property {propertyName} not found", HttpStatusCode.BadRequest);

            SortDirection dir = SortDirection.Desc;
            Expression<Func<TItem, object>>? keySelector =
                prop.GetCustomAttribute<SortableAttribute>()
                    ?.BuildKeySelector<TItem>(prop, propertyName, value, out dir) ??
                prop.GetCustomAttribute<CustomSortingAttribute<TItem>>()
                    ?.KeySelector(prop, propertyName, value, out dir);

            MjolnirException.ThrowIfNull(
                keySelector,
                $"No sorting attribute found for property {propertyName}",
                HttpStatusCode.BadRequest
            );

            ordered = first ? sortFunc(source, keySelector, dir) : thenFunc(ordered!, keySelector, dir);
            first = false;
        }

        return ordered!;
    }

    /// <summary>
    ///     Sorts a queryable sequence using the specified sort string.
    ///     Multiple sorts are applied in the order specified (primary, secondary, etc.).
    /// </summary>
    /// <typeparam name="T">The type of items in the queryable sequence.</typeparam>
    /// <param name="source">The source queryable sequence to sort.</param>
    /// <param name="sortString">The sort specification string in format: "property:asc|desc,property:asc|desc,..."</param>
    /// <returns>An ordered queryable sequence with the sort conditions applied.</returns>
    public static IOrderedQueryable<T> SortBy<T>(this IQueryable<T> source, string sortString) => source
        .SplitAndApply<IQueryable<T>, IOrderedQueryable<T>, T>(
            sortString,
            (src, selector, dir) => dir is SortDirection.Asc ? src.OrderBy(selector) : src.OrderByDescending(selector),
            (src, selector, dir) => dir is SortDirection.Asc ? src.ThenBy(selector) : src.ThenByDescending(selector)
        );

    /// <summary>
    ///     Sorts an enumerable sequence using the specified sort string.
    ///     Multiple sorts are applied in the order specified (primary, secondary, etc.).
    /// </summary>
    /// <typeparam name="T">The type of items in the enumerable sequence.</typeparam>
    /// <param name="source">The source enumerable sequence to sort.</param>
    /// <param name="sortString">The sort specification string in format: "property:asc|desc,property:asc|desc,..."</param>
    /// <returns>An ordered enumerable sequence with the sort conditions applied.</returns>
    public static IOrderedEnumerable<T> SortBy<T>(this IEnumerable<T> source, string sortString) => source
        .SplitAndApply<IEnumerable<T>, IOrderedEnumerable<T>, T>(
            sortString,
            (src, selector, dir) => dir is SortDirection.Asc ?
                src.OrderBy(selector.Compile()) :
                src.OrderByDescending(selector.Compile()),
            (src, selector, dir) => dir is SortDirection.Asc ?
                src.ThenBy(selector.Compile()) :
                src.ThenByDescending(selector.Compile())
        );
}
