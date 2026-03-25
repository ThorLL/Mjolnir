using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Reflection;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

/// <summary>
///     Abstract base class for range-based filtering attributes. Supports filtering by numeric ranges
///     with inclusive bounds and wildcard support for unbounded ranges.
/// </summary>
public abstract class RangeFilterAttribute : FilterableAttribute
{
    /// <summary>
    ///     Gets the minimum value for the range (used when wildcard * is specified).
    /// </summary>
    protected abstract object MinValue { get; }

    /// <summary>
    ///     Gets the maximum value for the range (used when wildcard * is specified).
    /// </summary>
    protected abstract object MaxValue { get; }

    /// <summary>
    ///     Gets a description of how to filter using ranges.
    /// </summary>
    /// <param name="prop">The property information.</param>
    /// <returns>A user-friendly description of the range filter syntax.</returns>
    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return
            $"Filter by `{propName}` using ranges: `filterBy={propName}:min-max` (inclusive) or `filterBy={propName}:min-*` / `filterBy={propName}:*-max`.";
    }

    /// <summary>
    ///     Builds a filter predicate that checks if a numeric property falls within the specified range (inclusive).
    /// </summary>
    /// <typeparam name="T">The type of entity to be filtered.</typeparam>
    /// <param name="prop">The property information being filtered.</param>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="valueString">The range specification in format "min-max" where * can be used for unbounded sides.</param>
    /// <returns>An expression predicate for range checking.</returns>
    /// <exception cref="MjolnirException">Thrown if the range format is invalid or values cannot be converted.</exception>
    public override Expression<Func<T, bool>> BuildPredicate<T>(
        PropertyInfo prop,
        string propertyName,
        string valueString
    )
    {
        string[] parts = valueString.Split('-');
        MjolnirException.ThrowIfNotEqual(parts.Length, 2, "Range filter must be in format 'min-max'");

        string minStr = parts[0];
        string maxStr = parts[1];
        MjolnirException.ThrowIfNullOrWhiteSpace(minStr);
        MjolnirException.ThrowIfNullOrWhiteSpace(maxStr);
        try
        {
            object minValue = minStr == "*" ? MinValue : Convert.ChangeType(minStr, prop.PropertyType);
            object maxValue = maxStr == "*" ? MaxValue : Convert.ChangeType(maxStr, prop.PropertyType);

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            MemberExpression property = Expression.Property(parameter, propertyName);

            ConstantExpression minConstant = Expression.Constant(minValue, property.Type);
            ConstantExpression maxConstant = Expression.Constant(maxValue, property.Type);

            BinaryExpression greaterThanOrEqual = Expression.GreaterThanOrEqual(property, minConstant);
            BinaryExpression lessThanOrEqual = Expression.LessThanOrEqual(property, maxConstant);

            BinaryExpression andExpression = Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }
        catch (FormatException ex)
        {
            throw new MjolnirException(
                $"Invalid range format: {ex.Message}",
                HttpStatusCode.BadRequest,
                ex
            );
        }
    }
}

/// <summary>
///     Generic range filter attribute for numeric types that implement <see cref="IMinMaxValue{T}" />.
/// </summary>
/// <typeparam name="T">The numeric type to filter by, must implement <see cref="IMinMaxValue{T}" /> and <see cref="IConvertible" />.</typeparam>
public sealed class RangeFilterAttribute<T> : RangeFilterAttribute
    where T : IMinMaxValue<T>, IConvertible
{
    /// <summary>
    ///     Gets the minimum value supported by the numeric type.
    /// </summary>
    protected override object MinValue => T.MinValue;

    /// <summary>
    ///     Gets the maximum value supported by the numeric type.
    /// </summary>
    protected override object MaxValue => T.MaxValue;
}
