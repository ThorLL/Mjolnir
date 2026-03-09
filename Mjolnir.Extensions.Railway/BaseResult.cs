using System.Runtime.CompilerServices;

namespace Mjolnir.Extensions.Railway;

public static class Result
{
    /// <summary>
    ///     Creates a success result containing the specified <paramref name="value" />.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the failure value.</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>An instance that encapsulates the given <paramref name="value" /> as success</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TFailure> Success<TSuccess, TFailure>(TSuccess value) => new(value);

    /// <summary>
    ///     Creates a failure result containing the specified <paramref name="exception" />.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the failure value.</typeparam>
    /// <param name="exception">The failure value (often an Exception).</param>
    /// <returns>An instance that encapsulates the given <paramref name="exception" /> as failure</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TFailure> Failure<TSuccess, TFailure>(TFailure exception) => new(exception);

    /// <summary>
    ///     Calls the specified function <paramref name="func" /> and returns its encapsulated result if invocation was
    ///     successful, catching any <see cref="Exception" /> that was thrown from the <paramref name="func" /> function
    ///     execution and encapsulating it as a failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A <see cref="Result{T, Exception}" /> containing the success value or the caught exception.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, Exception> RunCatching<T>(Func<T> func)
    {
        try
        {
            return Success<T, Exception>(func());
        }
        catch (Exception exception)
        {
            return Failure<T, Exception>(exception);
        }
    }

    /// <summary>
    ///     Calls the specified function <paramref name="func" /> asynchronously and returns its encapsulated result if
    ///     invocation was
    ///     successful, catching any <see cref="Exception" /> that was thrown from the <paramref name="func" /> function
    ///     execution and encapsulating it as a failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A <see cref="Result{T, Exception}" /> containing the success value or the caught exception.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T, Exception>> RunCatchingAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return Success<T, Exception>(await func());
        }
        catch (Exception exception)
        {
            return Failure<T, Exception>(exception);
        }
    }
}
