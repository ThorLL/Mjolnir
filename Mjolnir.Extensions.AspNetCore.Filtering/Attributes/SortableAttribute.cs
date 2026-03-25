using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

/// <summary>
///     Attribute for marking properties as sortable. Provides default sorting behavior with validation
///     that the property type implements <see cref="IComparable" /> or <see cref="IComparable{T}" />.
/// </summary>
public class SortableAttribute : BaseAttribute
{
    /// <summary>
    ///     Builds a key selector expression for sorting an entity by the specified property.
    /// </summary>
    /// <typeparam name="T">The type of entity to be sorted.</typeparam>
    /// <param name="prop">The property information being sorted.</param>
    /// <param name="propertyName">The name of the property to sort by.</param>
    /// <param name="valueString">The sort direction as a string (typically "asc" or "desc").</param>
    /// <param name="direction">The parsed sort direction.</param>
    /// <returns>An expression that selects the property value as the sort key.</returns>
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

    /// <summary>
    ///     Validates that the property is sortable by ensuring it implements <see cref="IComparable" />
    ///     or <see cref="IComparable{T}" />.
    /// </summary>
    /// <param name="prop">The property information to validate.</param>
    /// <exception cref="MjolnirException">Thrown if the property does not implement comparable interfaces.</exception>
    public override void Validate(PropertyInfo prop)
    {
        MjolnirException.ThrowIfNull(prop, statusCode: HttpStatusCode.BadRequest);

        Type type = prop.PropertyType;

        // Check if the property type implements IComparable<T>
        Type comparableGeneric = typeof(IComparable<>).MakeGenericType(type);
        bool implementsComparableGeneric = comparableGeneric.IsAssignableFrom(type);

        // Check if it implements non-generic IComparable
        bool implementsComparable = typeof(IComparable).IsAssignableFrom(type);

        MjolnirException.ThrowIfFalse(
            implementsComparable || implementsComparableGeneric,
            $"Property '{prop.Name}' of type '{type.FullName}' " +
            $"must implement IComparable<{type.Name}> or IComparable " +
            $"to be marked as [Sortable].",
            HttpStatusCode.InternalServerError
        );
    }

    /// <summary>
    ///     Gets a description of how to use sorting on this property.
    /// </summary>
    /// <param name="prop">The property information.</param>
    /// <returns>A user-friendly description of the sort syntax.</returns>
    public override string GetDescription(PropertyInfo prop) =>
        $"Sort by `{prop.Name}` in ascending or descending order: `sortBy={prop.Name}:asc|desc`.";
}
