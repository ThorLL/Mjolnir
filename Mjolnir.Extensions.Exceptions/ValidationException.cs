namespace Mjolnir.Extensions.Exceptions;

/// <summary>
///     Exception for collecting and reporting multiple validation errors.
///     Allows accumulating errors from multiple validation checks before throwing.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    ///     Gets the collection of validation errors.
    /// </summary>
    public ICollection<Error> Errors { get; init; } = new List<Error>();

    /// <summary>
    ///     Adds a validation error for a specific property.
    /// </summary>
    /// <param name="propertyName">The name of the property that failed validation.</param>
    /// <param name="errorMessage">The validation error message.</param>
    /// <returns>This exception instance for fluent chaining.</returns>
    public ValidationException WithError(string propertyName, string errorMessage)
    {
        Errors.Add(new Error(propertyName, errorMessage));
        return this;
    }

    /// <summary>
    ///     Throws this exception if any validation errors have been collected.
    /// </summary>
    /// <exception cref="ValidationException">Thrown if <see cref="Errors" /> is not empty.</exception>
    public void ThrowIfHasError()
    {
        if (Errors.Count != 0) throw this;
    }
}

/// <summary>
///     Represents a single validation error for a property.
/// </summary>
/// <param name="PropertyName">The name of the property that failed validation.</param>
/// <param name="ErrorMessage">The validation error message.</param>
public record Error(string PropertyName, string ErrorMessage);
