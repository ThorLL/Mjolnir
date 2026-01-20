using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public abstract class BaseAttribute : Attribute
{
    public virtual void Validate(PropertyInfo prop)
    {
    }
    public abstract string GetDescription(PropertyInfo prop);
}
