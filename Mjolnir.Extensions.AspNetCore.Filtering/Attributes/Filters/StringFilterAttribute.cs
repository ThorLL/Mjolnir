using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

/// <summary>
///     Attribute for filtering string properties by substring matching.
///     Multiple filter values are combined with OR logic.
/// </summary>
public sealed class StringFilterAttribute : FilterableAttribute
{
    /// <summary>
    ///     Gets a description of how to filter using substring matching.
    /// </summary>
    /// <param name="prop">The property information.</param>
    /// <returns>A user-friendly description of the string filter syntax.</returns>
    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return
            $"Filter by `{propName}` using substring match: `filterBy={propName}:value`. Multiple values can be combined with `|` (OR logic).";
    }

    /// <summary>
    ///     Builds a filter predicate that checks if a string property contains any of the specified substrings.
    ///     Multiple values are combined with OR logic.
    /// </summary>
    /// <typeparam name="T">The type of entity to be filtered.</typeparam>
    /// <param name="prop">The property information being filtered.</param>
    /// <param name="propertyName">The name of the string property to filter.</param>
    /// <param name="valueString">A pipe-separated string of substrings to match.</param>
    /// <returns>An expression predicate for substring matching with OR logic.</returns>
    /// <exception cref="MjolnirException">Thrown if any value is null or whitespace.</exception>
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
                ConstantExpression constant = Expression.Constant(v);
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                MethodCallExpression containsCall = Expression.Call(property, containsMethod, constant);

                return Expression.Lambda<Func<T, bool>>(containsCall, parameter);
            })
            .AsOrExpression();
    }
}
