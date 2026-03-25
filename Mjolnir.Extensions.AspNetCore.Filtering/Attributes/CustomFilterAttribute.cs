using System.Linq.Expressions;
using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

/// <summary>
///     Abstract base class for custom filter attributes that define filtering predicates.
///     Implementers should provide a predicate expression for filtering entities of type <typeparamref name="T" />.
/// </summary>
/// <typeparam name="T">The type of entity to be filtered.</typeparam>
public abstract class CustomFilterAttribute<T> : BaseAttribute
{
    /// <summary>
    ///     Builds a filter predicate expression based on the property and filter value.
    /// </summary>
    /// <param name="prop">The property information being filtered.</param>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="valueString">The string representation of the filter value.</param>
    /// <returns>An expression predicate that defines the filter condition.</returns>
    public abstract Expression<Func<T, bool>> Predicate(PropertyInfo prop, string propertyName, string valueString);
}
