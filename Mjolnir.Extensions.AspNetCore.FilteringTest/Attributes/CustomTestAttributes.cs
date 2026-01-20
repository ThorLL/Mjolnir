using System.Linq.Expressions;
using System.Reflection;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.AspNetCore.Filtering.Domain;
using Mjolnir.Extensions.AspNetCore.FilteringTest.Models;

namespace Mjolnir.Extensions.AspNetCore.FilteringTest.Attributes;

// Custom filter: matches NameAlias starting with any of the provided prefixes (OR with '|')
public sealed class CustomStartsWithFilterAttribute : CustomFilterAttribute<TestProduct>
{
    public override string GetDescription(PropertyInfo prop) => "Custom test filter: starts with (supports v1|v2 OR)";

    public override Expression<Func<TestProduct, bool>> Predicate(PropertyInfo prop, string propertyName, string valueString)
    {
        // Build OR expression: x => x.NameAlias.StartsWith(v1) || x.NameAlias.StartsWith(v2) ...
        ParameterExpression parameter = Expression.Parameter(typeof(TestProduct), "x");
        MemberExpression property = Expression.Property(parameter, nameof(TestProduct.NameAlias));
        MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;

        return valueString
            .Split('|')
            .Select(v =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(v);
                ConstantExpression constant = Expression.Constant(v);
                MethodCallExpression call = Expression.Call(property, startsWithMethod, constant);
                return Expression.Lambda<Func<TestProduct, bool>>(call, parameter);
            })
            .AsOrExpression();
    }
}

// Custom sorting: sorts by Name length; direction is parsed from valueString ("asc" or "desc")
public sealed class CustomNameLengthSortAttribute : CustomSortingAttribute<TestProduct>
{
    public override string GetDescription(PropertyInfo prop) => "Custom test sort: by Name length (asc|desc)";

    public override Expression<Func<TestProduct, object>> KeySelector(PropertyInfo prop, string propertyName, string valueString, out SortDirection direction)
    {
        direction = valueString.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? SortDirection.Desc
            : SortDirection.Asc;

        // key selector: x => (object)x.Name.Length
        ParameterExpression parameter = Expression.Parameter(typeof(TestProduct), "x");
        MemberExpression nameProp = Expression.Property(parameter, nameof(TestProduct.Name));
        MemberExpression lengthProp = Expression.Property(nameProp, nameof(string.Length));
        UnaryExpression box = Expression.Convert(lengthProp, typeof(object));
        return Expression.Lambda<Func<TestProduct, object>>(box, parameter);
    }
}
