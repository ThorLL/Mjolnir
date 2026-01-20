using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

public class SortableAttribute : BaseAttribute
{
    public virtual Expression<Func<T, object>> BuildKeySelector<T>(
        PropertyInfo prop,
        string propertyName,
        string valueString,
        out SortDirection direction
    )
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        MemberExpression property = Expression.Property(parameter, propertyName);
        UnaryExpression converted = Expression.Convert(property, typeof(object));
        Expression<Func<T, object>> propertyExpression = Expression.Lambda<Func<T, object>>(converted, parameter);
        direction = Enum.Parse<SortDirection>(valueString, true);
        return propertyExpression;
    }

    public override void Validate(PropertyInfo prop)
    {
        ArgumentNullException.ThrowIfNull(prop);

        Type type = prop.PropertyType;

        // Check if the property type implements IComparable<T>
        Type comparableGeneric = typeof(IComparable<>).MakeGenericType(type);
        bool implementsComparableGeneric = comparableGeneric.IsAssignableFrom(type);

        // Check if it implements non-generic IComparable
        bool implementsComparable = typeof(IComparable).IsAssignableFrom(type);

        if (!implementsComparable && !implementsComparableGeneric)
            throw new InvalidOperationException(
                $"Property '{prop.Name}' of type '{type.FullName}' " +
                $"must implement IComparable<{type.Name}> or IComparable " +
                $"to be marked as [Sortable]."
            );
    }

    public override string GetDescription(PropertyInfo prop) =>
        $"Sort by `{prop.Name}` in ascending or descending order: `sortBy={prop.Name}:asc|desc`.";
}

