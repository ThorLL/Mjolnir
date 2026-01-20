using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Attributes;


public abstract class CustomSortingAttribute<TSource> : BaseAttribute
{
    public abstract Expression<Func<TSource, object>> KeySelector(
        PropertyInfo prop,
        string propertyName,
        string valueString,
        out SortDirection direction
    );
}

