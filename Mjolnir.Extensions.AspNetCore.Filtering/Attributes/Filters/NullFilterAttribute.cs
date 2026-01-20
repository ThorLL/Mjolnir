using System.Linq.Expressions;
using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

public sealed class NullFilterAttribute : FilterableAttribute
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

        ConstantExpression nullConstant = Expression.Constant(null, typeof(object));
        Expression propAsObject = Expression.Convert(property, typeof(object));

        BinaryExpression notEqualExpr = Expression.NotEqual(propAsObject, nullConstant);
        ConstantExpression valueConstant = Expression.Constant(val, typeof(bool));
        BinaryExpression comparisonExpr = Expression.Equal(notEqualExpr, valueConstant);

        return Expression.Lambda<Func<T, bool>>(comparisonExpr, parameter);
    }

    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return $"Filter by `{propName}` for nullability: `filterBy={propName}:true|false` (true: NOT NULL, false: NULL).";
    }
}
