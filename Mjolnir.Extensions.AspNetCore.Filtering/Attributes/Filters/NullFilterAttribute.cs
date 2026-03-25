using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

/// <summary>
///     Attribute for filtering properties by nullability. Filters entities based on whether
///     a property value is null or not null.
/// </summary>
public sealed class NullFilterAttribute : FilterableAttribute
{
    /// <summary>
    ///     Builds a filter predicate that checks the nullability of a property.
    /// </summary>
    /// <typeparam name="T">The type of entity to be filtered.</typeparam>
    /// <param name="prop">The property information being filtered.</param>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="valueString">Boolean string indicating filter direction ("true" for NOT NULL, "false" for NULL).</param>
    /// <returns>An expression predicate for null checking.</returns>
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

        ConstantExpression nullConstant = Expression.Constant(null, typeof(object));
        Expression propAsObject = Expression.Convert(property, typeof(object));

        BinaryExpression notEqualExpr = Expression.NotEqual(propAsObject, nullConstant);
        ConstantExpression valueConstant = Expression.Constant(val, typeof(bool));
        BinaryExpression comparisonExpr = Expression.Equal(notEqualExpr, valueConstant);

        return Expression.Lambda<Func<T, bool>>(comparisonExpr, parameter);
    }

    /// <summary>
    ///     Gets a description of how to filter by nullability.
    /// </summary>
    /// <param name="prop">The property information.</param>
    /// <returns>A user-friendly description of the null filter syntax.</returns>
    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return
            $"Filter by `{propName}` for nullability: `filterBy={propName}:true|false` (true: NOT NULL, false: NULL).";
    }
}
