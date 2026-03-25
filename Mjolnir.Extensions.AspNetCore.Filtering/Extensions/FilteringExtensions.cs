using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

/// <summary>
///     Extension methods for filtering sequences of entities using filter strings.
///     Supports both <see cref="IQueryable{T}" /> and <see cref="IEnumerable{T}" /> sources.
/// </summary>
public static class FilteringExtensions
{
    /// <summary>
    ///     Applies filtering to a sequence by parsing a filter string and applying matching attribute predicates.
    ///     Filter string format: "property1:value1,property2:value2,..."
    /// </summary>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TItem">The type of items in the source sequence.</typeparam>
    /// <param name="items">The source sequence to filter.</param>
    /// <param name="filterString">The filter specification string.</param>
    /// <param name="filterFunc">A function that applies a predicate to the source sequence.</param>
    /// <returns>The filtered sequence with all filter conditions applied.</returns>
    /// <exception cref="MjolnirException">Thrown if a property is not found, has no filter attribute, or has invalid values.</exception>
    private static TSource SplitAndApply<TSource, TItem>(
        this TSource items,
        string filterString,
        Func<TSource, Expression<Func<TItem, bool>>, TSource> filterFunc
    )
    {
        StringBuilder sb = new();
        foreach (char c in filterString.Where(c => !char.IsWhiteSpace(c))) sb.Append(c);
        string[] filters = sb.ToString().Split(',');
        foreach (string filter in filters)
        {
            string[] parts = filter.Split(':');
            string propertyName = parts[0];
            string value = parts[1];
            MjolnirException.ThrowIfNullOrWhiteSpace(propertyName);
            MjolnirException.ThrowIfNullOrWhiteSpace(value);
            PropertyInfo? prop = typeof(TItem).GetProperty(propertyName);
            MjolnirException.ThrowIfNull(prop, $"Property {propertyName} not found", HttpStatusCode.BadRequest);

            Expression<Func<TItem, bool>>? predicate =
                prop.GetCustomAttribute<FilterableAttribute>()?.BuildPredicate<TItem>(prop, propertyName, value) ??
                prop.GetCustomAttribute<CustomFilterAttribute<TItem>>()?.Predicate(prop, propertyName, value);

            MjolnirException.ThrowIfNull(predicate,
                $"No filtering attribute found for property {propertyName}",
                HttpStatusCode.BadRequest
            );

            items = filterFunc(items, predicate);
        }

        return items;
    }

    extension<T>(IQueryable<T> items) where T : notnull
    {
        /// <summary>
        ///     Filters a queryable sequence using the specified filter string.
        ///     Multiple filters are combined with AND logic.
        /// </summary>
        /// <param name="filterString">
        ///     The filter specification string in format: "property:value,property:value,..."
        ///     Individual properties can support multiple values with OR logic using "|" separator.
        /// </param>
        /// <returns>A queryable sequence with the filter conditions applied.</returns>
        public IQueryable<T> FilterBy(string filterString) => items.SplitAndApply<IQueryable<T>, T>(filterString,
            (src, predicate) => src.Where(predicate)
        );
    }

    extension<T>(IEnumerable<T> items) where T : notnull
    {
        /// <summary>
        ///     Filters an enumerable sequence using the specified filter string.
        ///     Multiple filters are combined with AND logic.
        /// </summary>
        /// <param name="filterString">
        ///     The filter specification string in format: "property:value,property:value,..."
        ///     Individual properties can support multiple values with OR logic using "|" separator.
        /// </param>
        /// <returns>An enumerable sequence with the filter conditions applied.</returns>
        public IEnumerable<T> FilterBy(string filterString) => items.SplitAndApply<IEnumerable<T>, T>(filterString,
            (src, predicate) => src.Where(predicate.Compile())
        );
    }
}
