using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

/// <summary>
///     Attribute for filtering properties by boolean values. Filters entities where the property
///     exactly matches the specified boolean value (true or false).
/// </summary>
public sealed class BoolFilterAttribute : FilterableAttribute
{
    /// <summary>
    ///     Builds a filter predicate that checks if a boolean property matches the specified value.
    /// </summary>
    /// <typeparam name="T">The type of entity to be filtered.</typeparam>
    /// <param name="prop">The property information being filtered.</param>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="valueString">The string representation of the boolean value ("true" or "false").</param>
    /// <returns>An expression predicate for boolean equality.</returns>
    /// <exception cref="MjolnirException">Thrown if the value is not a valid boolean.</exception>
    public override Expression<Func<T, bool>> BuildPredicate<T>(
        PropertyInfo prop,
        string propertyName,
        string valueString
    )
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(parameter, propertyName);

        MjolnirException.ThrowIfNullOrWhiteSpace(valueString);
        MjolnirException.ThrowIfFalse(
            bool.TryParse(valueString, out bool val),
            $"Value '{valueString}' is not a valid boolean.",
            HttpStatusCode.BadRequest
        );

        ConstantExpression constant = Expression.Constant(val, typeof(bool));
        BinaryExpression comparisonExpr = Expression.Equal(property, constant);

        return Expression.Lambda<Func<T, bool>>(comparisonExpr, parameter);
    }

    /// <summary>
    ///     Gets a description of how to filter using boolean values.
    /// </summary>
    /// <param name="prop">The property information.</param>
    /// <returns>A user-friendly description of the boolean filter syntax.</returns>
    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return $"Filter by `{propName}` using boolean values: `filterBy={propName}:true|false`.";
    }
}
