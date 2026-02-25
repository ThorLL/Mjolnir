using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

public abstract class RangeFilterAttribute : FilterableAttribute
{
    protected abstract object MinValue { get; }
    protected abstract object MaxValue { get; }

    public override string GetDescription(PropertyInfo prop)
    {
        string propName = prop.Name;
        return $"Filter by `{propName}` using ranges: `filterBy={propName}:min-max` (inclusive) or `filterBy={propName}:min-*` / `filterBy={propName}:*-max`.";
    }

    public override Expression<Func<T, bool>> BuildPredicate<T>(
        PropertyInfo prop,
        string propertyName,
        string valueString
    )
    {
        string[] parts = valueString.Split('-');
        if (parts.Length != 2) throw new ArgumentException("Range filter must be in format 'min-max'");

        string minStr = parts[0];
        string maxStr = parts[1];
        ArgumentException.ThrowIfNullOrWhiteSpace(minStr);
        ArgumentException.ThrowIfNullOrWhiteSpace(maxStr);
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
            throw new ArgumentException($"Invalid range format: {ex.Message}", ex);
        }
    }
}

public sealed class RangeFilterAttribute<T> : RangeFilterAttribute
    where T : IMinMaxValue<T>, IConvertible
{
    protected override object MinValue => T.MinValue;
    protected override object MaxValue => T.MaxValue;
}
