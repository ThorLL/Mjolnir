using System.Linq.Expressions;
using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

public sealed class BoolFilterAttribute : FilterableAttribute
{
    public override Expression<Func<T, bool>> BuildPredicate<T>(
        PropertyInfo prop,
        string propertyName,
        string valueString
    )
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(parameter, propertyName);

        ArgumentException.ThrowIfNullOrWhiteSpace(valueString);
        if (!bool.TryParse(valueString, out bool val))
            throw new ArgumentException($"Value '{valueString}' is not a valid boolean.");

        ConstantExpression constant = Expression.Constant(val, typeof(bool));
        BinaryExpression comparisonExpr = Expression.Equal(property, constant);

        return Expression.Lambda<Func<T, bool>>(comparisonExpr, parameter);
    }

    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return $"Filter by `{propName}` using boolean values: `filterBy={propName}:true|false`.";
    }
}
