using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Mjolnir.Extensions.Railway;

/// <summary>
///     A discriminated union that encapsulates a successful outcome with a value of type
///     <typeparamref name="TSuccess"></typeparamref> or a failure with an
///     arbitrary <see cref="Exception" />
/// </summary>
public readonly struct Result<TSuccess, TFailure>
{
    internal readonly object? Value;

    internal Result(object? value)
    {
        Value = value;
    }

    /// <summary>
    ///     Returns <c>true</c> if this instance represents a failed outcome. In this case <see cref="IsSuccess" /> returns
    ///     <c>false</c>.
    /// </summary>
    public bool IsFailure => Value is TFailure;

    /// <summary>
    ///     Returns <c>true</c> if this instance represents a successful outcome. In this case <see cref="IsFailure" /> returns
    ///     <c>false</c>.
    /// </summary>
    public bool IsSuccess => !IsFailure;

    /// <summary>
    ///     Tries to get the failure value if this instance represents a failure.
    /// </summary>
    /// <param name="failure">The failure value if the result is a failure; otherwise, default.</param>
    /// <returns><c>true</c> if the result is a failure; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetFailure([MaybeNullWhen(false)] out TFailure failure)
    {
        if (Value is TFailure failureValue)
        {
            failure = failureValue;
            return true;
        }

        failure = default;
        return false;
    }

    /// <summary>
    ///     Returns a string representation of the result.
    /// </summary>
    /// <returns>A string <c>Success(v)</c> if this instance represents success or <c>Failure(x)</c> if it is failure.</returns>
    public override string ToString() => Value switch
    {
        TFailure failure => $"Failure({failure})",
        _ => $"Success({Value})"
    };

    /// <summary>
    ///     Tries to get the success value if this instance represents success.
    /// </summary>
    /// <param name="success">The success value if the result is success; otherwise, default.</param>
    /// <returns><c>true</c> if the result is success; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetSuccess([MaybeNullWhen(false)] out TSuccess success)
    {
        if (Value is TSuccess successValue)
        {
            success = successValue;
            return true;
        }

        success = default;
        return false;
    }

    /// <summary>
    ///     Unfolds the result into success and failure components.
    /// </summary>
    /// <param name="success">The success value if success.</param>
    /// <param name="failure">The failure value if failure.</param>
    /// <returns><c>true</c> if success; <c>false</c> if failure.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is in an invalid state.</exception>
    public bool Unfold(
        [MaybeNullWhen(false)] out TSuccess success,
        [MaybeNullWhen(true)] out TFailure failure
    )
    {
        switch (Value)
        {
            case TSuccess successValue:
                success = successValue;
                failure = default;
                return true;
            case TFailure failureValue:
                success = default;
                failure = failureValue;
                return false;
            default:
                throw new InvalidOperationException("Result is neither success nor failure");
        }
    }
}
