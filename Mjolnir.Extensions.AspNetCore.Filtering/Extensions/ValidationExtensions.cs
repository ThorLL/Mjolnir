using System.Net;
using Mjolnir.Extensions.Exceptions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

/// <summary>
///     Extension methods for validating sequences have unique values.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    ///     Validates that all items in the sequence are unique using their natural comparison.
    /// </summary>
    /// <typeparam name="T">The type of items. Must implement <see cref="IComparable" />.</typeparam>
    /// <param name="source">The sequence to validate.</param>
    /// <returns>The original sequence if all items are unique.</returns>
    /// <exception cref="MjolnirException">Thrown if duplicate values are found.</exception>
    public static IEnumerable<T> MustBeUnique<T>(this IEnumerable<T> source)
        where T : IComparable => source.MustBeUnique(s => s);

    /// <summary>
    ///     Validates that all items in the sequence are unique based on a key selector function.
    /// </summary>
    /// <typeparam name="T">The type of items in the sequence.</typeparam>
    /// <typeparam name="TKey">The type of the comparison key. Must implement <see cref="IComparable" />.</typeparam>
    /// <param name="source">The sequence to validate.</param>
    /// <param name="selector">A function to extract the comparison key from each item.</param>
    /// <returns>The original sequence if all keys are unique.</returns>
    /// <exception cref="MjolnirException">Thrown if duplicate keys are found.</exception>
    public static IEnumerable<T> MustBeUnique<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
        where TKey : IComparable
    {
        MjolnirException.ThrowIfNull(source, statusCode: HttpStatusCode.BadRequest);
        MjolnirException.ThrowIfNull(selector, statusCode: HttpStatusCode.BadRequest);

        List<TKey> seen = [];

        foreach (T item in source)
        {
            TKey value = selector(item);
            MjolnirException.ThrowIfTrue(
                seen.Any(s => s.CompareTo(value) == 0),
                $"Duplicate values found: {value}",
                HttpStatusCode.BadRequest
            );
            seen.Add(value);
            yield return item;
        }
    }
}
