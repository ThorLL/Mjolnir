using System.Linq.Expressions;
using System.Reflection;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

public abstract class CustomFilterAttribute<T> : BaseAttribute
{
    public abstract Expression<Func<T, bool>> Predicate(PropertyInfo prop, string propertyName, string valueString);
}
