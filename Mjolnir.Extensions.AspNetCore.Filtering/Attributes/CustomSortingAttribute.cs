using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

/// <summary>
///     Abstract base class for custom sorting attributes that define key selectors for sorting.
///     Implementers should provide a key selector expression to determine the sorting order of entities.
/// </summary>
/// <typeparam name="TSource">The type of entity to be sorted.</typeparam>
public abstract class CustomSortingAttribute<TSource> : BaseAttribute
{
    /// <summary>
    ///     Builds a key selector expression to determine the sorting order.
    /// </summary>
    /// <param name="prop">The property information being sorted.</param>
    /// <param name="propertyName">The name of the property to sort by.</param>
    /// <param name="valueString">The string representation of the sort value (e.g., direction).</param>
    /// <param name="direction">The sort direction (ascending or descending).</param>
    /// <returns>An expression that selects the key to sort by.</returns>
    public abstract Expression<Func<TSource, object>> KeySelector(
        PropertyInfo prop,
        string propertyName,
        string valueString,
        out SortDirection direction
    );
}

