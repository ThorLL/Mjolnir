using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

public static class FilteringExtensions
{
    private static TSource SplitAndApply<TSource, TItem>(
        this TSource items,
        string filterString,
        Func<TSource, Expression<Func<TItem, bool>>, TSource> filterFunc
    )
    {
        StringBuilder sb = new();
        foreach (char c in filterString.Where(c => !char.IsWhiteSpace(c))) sb.Append(c);
        string[] filters = sb.ToString().Split(',');
        foreach (string filter in filters)
        {
            string[] parts = filter.Split(':');
            string propertyName = parts[0];
            string value = parts[1];
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            PropertyInfo prop = typeof(TItem).GetProperty(propertyName) ??
                                throw new ArgumentException($"Property {propertyName} not found");

            Expression<Func<TItem, bool>> predicate =
                prop.GetCustomAttribute<FilterableAttribute>()?.BuildPredicate<TItem>(prop, propertyName, value) ??
                prop.GetCustomAttribute<CustomFilterAttribute<TItem>>()?.Predicate(prop, propertyName, value) ??
                throw new ArgumentException($"No filtering attribute found for property {propertyName}");

            items = filterFunc(items, predicate);
        }

        return items;
    }


    extension<T>(IQueryable<T> items) where T : notnull
    {
        public IQueryable<T> FilterBy(string filterString) => items.SplitAndApply<IQueryable<T>, T>(filterString,
            (src, predicate) => src.Where(predicate)
        );
    }

    extension<T>(IEnumerable<T> items) where T : notnull
    {
        public IEnumerable<T> FilterBy(string filterString) => items.SplitAndApply<IEnumerable<T>, T>(filterString,
           (src, predicate) => src.Where(predicate.Compile())
        );
    }
}
