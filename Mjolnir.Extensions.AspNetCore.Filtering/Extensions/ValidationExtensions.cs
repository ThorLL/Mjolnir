namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

public static class ValidationExtensions
{
    public static IEnumerable<T> MustBeUnique<T>(this IEnumerable<T> source)
        where T : IComparable => source.MustBeUnique(s => s);

    public static IEnumerable<T> MustBeUnique<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
        where TKey : IComparable
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        List<TKey> seen = [];

        foreach (T item in source)
        {
            TKey value = selector(item);
            if (seen.Any(s => s.CompareTo(value) == 0)) throw new ArgumentException($"Duplicate values found: {value}");
            seen.Add(value);
            yield return item;
        }
    }
}
