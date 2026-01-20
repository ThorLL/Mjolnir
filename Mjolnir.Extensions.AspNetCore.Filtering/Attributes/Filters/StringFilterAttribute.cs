using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

public sealed class StringFilterAttribute : FilterableAttribute
{
    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return $"Filter by `{propName}` using substring match: `filterBy={propName}:value`. Multiple values can be combined with `|` (OR logic).";
    }

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
                ArgumentException.ThrowIfNullOrWhiteSpace(v);
                ConstantExpression constant = Expression.Constant(v);
                MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                MethodCallExpression containsCall = Expression.Call(property, containsMethod, constant);

                return Expression.Lambda<Func<T, bool>>(containsCall, parameter);
            })
            .AsOrExpression();
    }
}
