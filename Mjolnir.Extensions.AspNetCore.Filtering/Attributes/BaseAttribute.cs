using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

/// <summary>
///     Base attribute class for filtering and sorting attributes applied to properties.
///     Provides validation and description generation functionality.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class BaseAttribute : Attribute
{
    /// <summary>
    ///     Validates whether the property is suitable for the filtering/sorting operation.
    ///     Override this method to provide custom validation logic.
    /// </summary>
    /// <param name="prop">The property information to validate.</param>
    public virtual void Validate(PropertyInfo prop)
    {
    }

    /// <summary>
    ///     Gets a human-readable description of how to use this attribute for filtering or sorting.
    /// </summary>
    /// <param name="prop">The property information to describe.</param>
    /// <returns>A description string explaining the usage of this attribute.</returns>
    public abstract string GetDescription(PropertyInfo prop);
}
