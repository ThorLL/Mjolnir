namespace Mjolnir.Extensions.AspNetCore.Filtering.Domain;

/// <summary>
///     Defines the sort direction for ordering operations.
/// </summary>
public enum SortDirection
{
    /// <summary>
    ///     Ascending order (A to Z, smallest to largest, earliest to latest).
    /// </summary>
    Asc,

    /// <summary>
    ///     Descending order (Z to A, largest to smallest, latest to earliest).
    /// </summary>
    Desc
}
