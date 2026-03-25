using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using ModelContextProtocol;
using static System.Net.HttpStatusCode;

namespace Mjolnir.Extensions.Exceptions;

/// <summary>
///     Custom exception for Mjolnir library operations with HTTP status code support.
///     Extends <see cref="McpException" /> for mcp server support and provide status codes for HTTP scenarios.
/// </summary>
public class MjolnirException(
    string message,
    HttpStatusCode statusCode = InternalServerError,
    Exception? innerException = null
) : McpException(message, innerException)
{
    /// <summary>
    ///     Initializes a new instance with a message and inner exception, using InternalServerError status.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public MjolnirException(string message, Exception innerException) : this(
        message,
        InternalServerError,
        innerException
    )
    {
    }

    /// <summary>
    ///     Gets the HTTP status code associated with this exception.
    /// </summary>
    public int StatusCode { get; } = (int)statusCode;

    /// <summary>
    ///     Throws this exception if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="condition" /> is <c>true</c>.</exception>
    public static void ThrowIfTrue([DoesNotReturnIf(true)] bool condition, string message, HttpStatusCode statusCode)
    {
        if (condition) throw new MjolnirException(message, statusCode);
    }

    /// <summary>
    ///     Throws this exception if the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="condition" /> is <c>false</c>.</exception>
    public static void ThrowIfFalse([DoesNotReturnIf(false)] bool condition, string message, HttpStatusCode statusCode)
        => ThrowIfTrue(!condition, message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified argument is null.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="argument" /> is <c>null</c>.</exception>
    public static void ThrowIfNull(
        [NotNull] object? argument,
        string? message = null,
        HttpStatusCode statusCode = NotFound,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null
    )
    {
        if (message is not null) ThrowIfTrue(argument is null, message, statusCode);

        try
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
        }
        catch (Exception e)
        {
            throw new MjolnirException(e.Message, statusCode, e);
        }
    }

    /// <summary>
    ///     Throws this exception if the specified string is null or empty.
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="argument" /> is <c>null</c> or empty.</exception>
    public static void ThrowIfNullOrEmpty(
        [NotNull] string? argument,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null
    )
    {
        if (message is not null) ThrowIfTrue(string.IsNullOrEmpty(argument), message, statusCode);

        try
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
        }
        catch (Exception e)
        {
            throw new MjolnirException(e.Message, statusCode, e);
        }
    }

    /// <summary>
    ///     Throws this exception if the specified string is null, empty, or consists of only whitespace.
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="argument" /> is <c>null</c>, empty, or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? argument,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(argument))]
        string? paramName = null
    )
    {
        if (message is not null) ThrowIfTrue(string.IsNullOrWhiteSpace(argument), message, statusCode);

        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
        }
        catch (Exception e)
        {
            throw new MjolnirException(e.Message, statusCode, e);
        }
    }

    /// <summary>
    ///     Throws this exception if the specified numeric value is zero.
    /// </summary>
    /// <typeparam name="T">The numeric type implementing <see cref="INumberBase{T}" />.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is zero.</exception>
    public static void ThrowIfZero<T>(
        T value,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : INumberBase<T>
    {
        if (message is not null) ThrowIfTrue(T.IsZero(value), message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfZero(value, paramName));
    }

    /// <summary>
    ///     Throws this exception if the specified numeric value is negative.
    /// </summary>
    /// <typeparam name="T">The numeric type implementing <see cref="INumberBase{T}" />.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is negative.</exception>
    public static void ThrowIfNegative<T>(
        T value,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
        where T : INumberBase<T>
    {
        if (message is not null) ThrowIfTrue(T.IsNegative(value), message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfNegative(value, paramName));
    }

    /// <summary>
    ///     Throws this exception if the specified numeric value is negative or zero.
    /// </summary>
    /// <typeparam name="T">The numeric type implementing <see cref="INumberBase{T}" />.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is negative or zero.</exception>
    public static void ThrowIfNegativeOrZero<T>(
        T value,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : INumberBase<T>
    {
        if (message is not null) ThrowIfTrue(T.IsNegative(value) || T.IsZero(value), message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName));
    }

    /// <summary>
    ///     Throws this exception if two values are equal.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> equals <paramref name="other" />.</exception>
    public static void ThrowIfEqual<T>(
        T value,
        T other,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
    {
        if (message is not null) ThrowIfTrue(EqualityComparer<T>.Default.Equals(value, other), message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfEqual(value, other, paramName));
    }

    /// <summary>
    ///     Throws this exception if two values are not equal.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> does not equal <paramref name="other" />.</exception>
    public static void ThrowIfNotEqual<T>(
        T value,
        T other,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
    {
        if (message is not null) ThrowIfTrue(!EqualityComparer<T>.Default.Equals(value, other), message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfNotEqual(value, other, paramName));
    }

    /// <summary>
    ///     Throws this exception if the first value is greater than the second.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is greater than <paramref name="other" />.</exception>
    public static void ThrowIfGreaterThan<T>(
        T value,
        T other,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
        where T : IComparable<T>
    {
        if (message is not null) ThrowIfTrue(value.CompareTo(other) > 0, message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName));
    }

    /// <summary>
    ///     Throws this exception if the first value is greater than or equal to the second.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is greater than or equal to <paramref name="other" />.</exception>
    public static void ThrowIfGreaterThanOrEqual<T>(
        T value,
        T other,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    ) where T : IComparable<T>
    {
        if (message is not null) ThrowIfTrue(value.CompareTo(other) >= 0, message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName));
    }

    /// <summary>
    ///     Throws this exception if the first value is less than the second.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is less than <paramref name="other" />.</exception>
    public static void ThrowIfLessThan<T>(
        T value,
        T other,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
        where T : IComparable<T>
    {
        if (message is not null) ThrowIfTrue(value.CompareTo(other) < 0, message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName));
    }

    /// <summary>
    ///     Throws this exception if the first value is less than or equal to the second.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">The custom exception message, or null to use the default argument validation message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is less than or equal to <paramref name="other" />.</exception>
    public static void ThrowIfLessThanOrEqual<T>(
        T value,
        T other,
        string? message = null,
        HttpStatusCode statusCode = BadRequest,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null
    )
        where T : IComparable<T>
    {
        if (message is not null) ThrowIfTrue(value.CompareTo(other) <= 0, message, statusCode);
        else Wrap(statusCode, () => ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, paramName));
    }

    /// <summary>
    ///     Throws this exception if the specified condition is true using a lazily-evaluated message.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">A function that generates the exception message (evaluated only if condition is true).</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="condition" /> is <c>true</c>.</exception>
    public static void ThrowIfTrue(
        [DoesNotReturnIf(true)] bool condition,
        Func<string> message,
        HttpStatusCode statusCode
    )
    {
        if (condition) throw new MjolnirException(message(), statusCode);
    }

    /// <summary>
    ///     Throws this exception if the specified condition is false using a lazily-evaluated message.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">A function that generates the exception message (evaluated only if condition is false).</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="condition" /> is <c>false</c>.</exception>
    public static void ThrowIfFalse(
        [DoesNotReturnIf(false)] bool condition,
        Func<string> message,
        HttpStatusCode statusCode
    ) => ThrowIfTrue(!condition, message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified argument is null using a lazily-evaluated message.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="argument" /> is <c>null</c>.</exception>
    public static void ThrowIfNull(
        [NotNull] object? argument,
        Func<string> message,
        HttpStatusCode statusCode = NotFound
    ) => ThrowIfTrue(argument is null, message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified string is null or empty using a lazily-evaluated message.
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="argument" /> is <c>null</c> or empty.</exception>
    public static void ThrowIfNullOrEmpty(
        [NotNull] string? argument,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) => ThrowIfTrue(string.IsNullOrEmpty(argument), message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified string is null, empty, or whitespace using a lazily-evaluated message.
    /// </summary>
    /// <param name="argument">The string to check.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="argument" /> is <c>null</c>, empty, or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? argument,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) => ThrowIfTrue(string.IsNullOrWhiteSpace(argument), message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified numeric value is zero using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The numeric type implementing <see cref="INumberBase{T}" />.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is zero.</exception>
    public static void ThrowIfZero<T>(
        T value,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : INumberBase<T> => ThrowIfTrue(T.IsZero(value), message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified numeric value is negative using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The numeric type implementing <see cref="INumberBase{T}" />.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is negative.</exception>
    public static void ThrowIfNegative<T>(
        T value,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : INumberBase<T> => ThrowIfTrue(T.IsNegative(value), message, statusCode);

    /// <summary>
    ///     Throws this exception if the specified numeric value is negative or zero using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The numeric type implementing <see cref="INumberBase{T}" />.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is negative or zero.</exception>
    public static void ThrowIfNegativeOrZero<T>(
        T value,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : INumberBase<T> => ThrowIfTrue(T.IsNegative(value) || T.IsZero(value), message, statusCode);

    /// <summary>
    ///     Throws this exception if two values are equal using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> equals <paramref name="other" />.</exception>
    public static void ThrowIfEqual<T>(
        T value,
        T other,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) => ThrowIfTrue(EqualityComparer<T>.Default.Equals(value, other), message, statusCode);

    /// <summary>
    ///     Throws this exception if two values are not equal using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> does not equal <paramref name="other" />.</exception>
    public static void ThrowIfNotEqual<T>(
        T value,
        T other,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) => ThrowIfTrue(!EqualityComparer<T>.Default.Equals(value, other), message, statusCode);

    /// <summary>
    ///     Throws this exception if the first value is greater than the second using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is greater than <paramref name="other" />.</exception>
    public static void ThrowIfGreaterThan<T>(
        T value,
        T other,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : IComparable<T> => ThrowIfTrue(value.CompareTo(other) > 0, message, statusCode);

    /// <summary>
    ///     Throws this exception if the first value is greater than or equal to the second using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is greater than or equal to <paramref name="other" />.</exception>
    public static void ThrowIfGreaterThanOrEqual<T>(
        T value,
        T other,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : IComparable<T> => ThrowIfTrue(value.CompareTo(other) >= 0, message, statusCode);

    /// <summary>
    ///     Throws this exception if the first value is less than the second using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is less than <paramref name="other" />.</exception>
    public static void ThrowIfLessThan<T>(
        T value,
        T other,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : IComparable<T> => ThrowIfTrue(value.CompareTo(other) < 0, message, statusCode);

    /// <summary>
    ///     Throws this exception if the first value is less than or equal to the second using a lazily-evaluated message.
    /// </summary>
    /// <typeparam name="T">The comparable type.</typeparam>
    /// <param name="value">The first value to compare.</param>
    /// <param name="other">The second value to compare.</param>
    /// <param name="message">A function that generates the exception message.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="value" /> is less than or equal to <paramref name="other" />.</exception>
    public static void ThrowIfLessThanOrEqual<T>(
        T value,
        T other,
        Func<string> message,
        HttpStatusCode statusCode = BadRequest
    ) where T : IComparable<T> => ThrowIfTrue(value.CompareTo(other) <= 0, message, statusCode);

    /// <summary>
    ///     Wraps an action in exception handling to convert standard exceptions to <see cref="MjolnirException" />.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to use for wrapped exceptions.</param>
    /// <param name="thrower">The action that may throw an exception.</param>
    /// <exception cref="MjolnirException">Thrown if <paramref name="thrower" /> throws any exception.</exception>
    private static void Wrap(HttpStatusCode statusCode, Action thrower)
    {
        try
        {
            thrower();
        }
        catch (Exception e)
        {
            throw new MjolnirException(e.Message, statusCode, e);
        }
    }
}
