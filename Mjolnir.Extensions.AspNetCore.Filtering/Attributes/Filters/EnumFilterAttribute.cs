using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;

public abstract class EnumFilterAttribute : FilterableAttribute
{
    public abstract IEnumerable<Enum> Options { get; }

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
                object value = Enum.Parse(prop.PropertyType, v, true);
                ConstantExpression constant = Expression.Constant(value, property.Type);
                BinaryExpression comparisonExpr = Expression.MakeBinary(ExpressionType.Equal, property, constant);
                return Expression.Lambda<Func<T, bool>>(comparisonExpr, parameter);
            })
            .AsOrExpression();
    }
}

public sealed class EnumFilterAttribute<T> : EnumFilterAttribute
    where T : struct, Enum
{
    public override IEnumerable<Enum> Options => Enum.GetValues<T>().Cast<Enum>();

    public override void Validate(PropertyInfo prop)
    {
        T[] options = Enum.GetValues<T>();
        ArgumentOutOfRangeException.ThrowIfLessThan(options.Length, 2);
        options.MustBeUnique();
    }

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

        return $"Filter by `{propName}` using enum `{typeName}` values: `filterBy={filterByPattern}`. Multiple values can be combined with `|` (OR logic).";
    }
}
