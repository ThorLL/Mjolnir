using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

/// <summary>
///     Abstract base class for enum-based filtering attributes.
///     Provides core filtering logic for enum properties.
/// </summary>
public abstract class EnumFilterAttribute : FilterableAttribute
{
    /// <summary>
    ///     Gets the available enum options for this filter.
    /// </summary>
    public abstract IEnumerable<Enum> Options { get; }

    /// <summary>
    ///     Builds a filter predicate that checks if an enum property matches any of the specified enum values.
    ///     Multiple values are combined with OR logic.
    /// </summary>
    /// <typeparam name="T">The type of entity to be filtered.</typeparam>
    /// <param name="prop">The property information being filtered.</param>
    /// <param name="propertyName">The name of the property to filter.</param>
    /// <param name="valueString">A pipe-separated string of enum values to match.</param>
    /// <returns>An expression predicate for enum value matching with OR logic.</returns>
    /// <exception cref="MjolnirException">Thrown if any value is not a valid enum value.</exception>
    public override Expression<Func<T, bool>> BuildPredicate<T>(
        PropertyInfo prop,
        string propertyName,
        string valueString
    )
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(parameter, propertyName);

        return valueString
            .Split('|')
            .Select(v =>
            {
                MjolnirException.ThrowIfNullOrWhiteSpace(v);
                object value = Enum.Parse(prop.PropertyType, v, true);
                ConstantExpression constant = Expression.Constant(value, property.Type);
                BinaryExpression comparisonExpr = Expression.MakeBinary(ExpressionType.Equal, property, constant);
                return Expression.Lambda<Func<T, bool>>(comparisonExpr, parameter);
            })
            .AsOrExpression();
    }
}

/// <summary>
///     Generic enum filter attribute for filtering properties by enum values.
///     Provides validation that the enum has at least two values and all values are unique.
/// </summary>
/// <typeparam name="T">The enum type to filter by.</typeparam>
public sealed class EnumFilterAttribute<T> : EnumFilterAttribute
    where T : struct, Enum
{
    /// <summary>
    ///     Gets all available values of the enum type.
    /// </summary>
    public override IEnumerable<Enum> Options => Enum.GetValues<T>().Cast<Enum>();

    /// <summary>
    ///     Validates that the enum type has at least two distinct values.
    /// </summary>
    /// <param name="prop">The property information to validate.</param>
    /// <exception cref="MjolnirException">Thrown if the enum has fewer than two values.</exception>
    public override void Validate(PropertyInfo prop)
    {
        T[] options = Enum.GetValues<T>();
        MjolnirException.ThrowIfLessThan(options.Length, 2);
        options.MustBeUnique();
    }

    /// <summary>
    ///     Gets a description of how to filter using enum values.
    /// </summary>
    /// <param name="prop">The property information.</param>
    /// <returns>A user-friendly description of the enum filter syntax with examples.</returns>
    public override string GetDescription(PropertyInfo prop)
    {
        T[] options = Enum.GetValues<T>();

        string typeName = typeof(T).Name;
        string propName = prop.Name;

        string filterByPattern = options.Length switch
        {
            2 => $"{propName}:{options[0]}",
            3 => $"{propName}:{options[0]}|{options[1]}",
            _ => $"{propName}:{options[0]}|{options[1]}|...{options[^1]}"
        };

        return
            $"Filter by `{propName}` using enum `{typeName}` values: `filterBy={filterByPattern}`. Multiple values can be combined with `|` (OR logic).";
    }
}
