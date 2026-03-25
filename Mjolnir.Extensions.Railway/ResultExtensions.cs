using System.Runtime.CompilerServices;

namespace Mjolnir.Extensions.Railway;

/// <summary>
///     Provides extension methods for railway-oriented programming Result handling.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    ///     Retrieves the success value of the encapsulated result, or throws the failure exception if the result is a failure.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the success value.</typeparam>
    /// <typeparam name="TFailure">The type of the failure exception, which must be a subclass of <see cref="Exception" />.</typeparam>
    /// <param name="result">
    ///     The task encapsulating the result from which to retrieve the success value or throw the failure
    ///     exception.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the success value if the operation
    ///     succeeds.
    /// </returns>
    public static TSuccess GetOrThrow<TSuccess, TFailure>(this Result<TSuccess, TFailure> result)
        where TFailure : Exception => result.Unfold(out TSuccess? success, out TFailure? failure) ?
        success :
        throw failure;

    extension<TSuccess, TFailure>(Result<TSuccess, TFailure> result)
    {
        /// <summary>
        ///     Returns the result of <paramref name="onSuccess" /> for the encapsulated value if this instance represents
        ///     <see cref="Result{TSuccess,TFailure}.IsSuccess" /> or the result of <paramref name="onFailure" /> function for the
        ///     encapsulated <typeparamref name="TFailure" /> if it is <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <typeparam name="TNew">The type of the result of the fold operation.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is success.</param>
        /// <param name="onFailure">The function to execute if the result is failure.</param>
        /// <returns>The result of either <paramref name="onSuccess" /> or <paramref name="onFailure" />.</returns>
        /// <remarks>
        ///     This function rethrows any exception thrown by <paramref name="onSuccess" /> or by
        ///     <paramref name="onFailure" /> function.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TNew Fold<TNew>(
            Func<TSuccess, TNew> onSuccess,
            Func<TFailure, TNew> onFailure
        ) => result.Unfold(out TSuccess? success, out TFailure? failure) switch
        {
            false => onSuccess(success),
            true => onFailure(failure)
        };

        /// <summary>
        ///     Performs the given <paramref name="action" /> on the encapsulated failure value if this instance
        ///     represents <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The original <c>Result</c> unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TSuccess, TFailure> OnFailure(Action<TFailure> action)
        {
            if (result.TryGetFailure(out TFailure? failure)) action(failure);
            return result;
        }

        /// <summary>
        ///     Performs the given <paramref name="action" /> on the encapsulated failure value if this instance
        ///     represents <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The original <c>Result</c> unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Result<TSuccess, TFailure>> OnFailureAsync(Func<TFailure, Task> action)
        {
            if (result.TryGetFailure(out TFailure? failure)) await action(failure).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        ///     Performs the given <paramref name="action" /> on the encapsulated success value if this instance represents
        ///     <see cref="Result{TSuccess,TFailure}.IsSuccess" />.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The original <c>Result</c> unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TSuccess, TFailure> OnSuccess(Action<TSuccess> action)
        {
            if (result.TryGetSuccess(out TSuccess? success)) action(success);
            return result;
        }

        /// <summary>
        ///     Performs the given <paramref name="action" /> on the encapsulated success value if this instance represents
        ///     <see cref="Result{TSuccess,TFailure}.IsSuccess" />.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The original <c>Result</c> unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Result<TSuccess, TFailure>> OnSuccessAsync(Func<TSuccess, Task> action)
        {
            if (result.TryGetSuccess(out TSuccess? success)) await action(success).ConfigureAwait(false);
            return result;
        }


        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     value if this instance represents <see cref="Result{TSuccess,TFailure}.IsSuccess" /> or the original encapsulated
        ///     failure value if it is <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <typeparam name="TNew">The type of the success value after transformation.</typeparam>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new <see cref="Result{TNew, TFailure}" />.</returns>
        /// <remarks>
        ///     This function rethrows any exception thrown by <paramref name="transform" /> function. See
        ///     <see cref="MapCatching" /> for an alternative that encapsulates exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TNew, TFailure> Map<TNew>(Func<TSuccess, TNew> transform) =>
            result.TryGetSuccess(out TSuccess? success) switch
            {
                true => Result.Success<TNew, TFailure>(transform(success)),
                _ => new Result<TNew, TFailure>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     value if this instance represents <see cref="Result{TSuccess,TFailure}.IsSuccess" /> or the original encapsulated
        ///     failure value if it is <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <typeparam name="TNew">The type of the success value after transformation.</typeparam>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new <see cref="Result{TNew, TFailure}" />.</returns>
        /// <remarks>
        ///     This function rethrows any exception thrown by <paramref name="transform" /> function. See
        ///     <see cref="MapCatchingAsync" /> for an alternative that encapsulates exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Result<TNew, TFailure>> MapAsync<TNew>(Func<TSuccess, Task<TNew>> transform) =>
            result.TryGetSuccess(out TSuccess? success) switch
            {
                true => Result.Success<TNew, TFailure>(await transform(success).ConfigureAwait(false)),
                _ => new Result<TNew, TFailure>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the
        ///     encapsulated value if this instance represents success or the original encapsulated failure value if failure.
        /// </summary>
        /// <typeparam name="TNew">The type of the success value after transformation.</typeparam>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new <see cref="Result{TNew, Exception}" />.</returns>
        /// <remarks>
        ///     This function catches any <see cref="Exception" /> thrown by <paramref name="transform" /> function and
        ///     encapsulates it as a failure.
        ///     See <see cref="Map" /> for an alternative that rethrows exceptions from `transform` function.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TNew, Exception> MapCatching<TNew>(Func<TSuccess, TNew> transform) =>
            result.TryGetSuccess(out TSuccess? success) switch
            {
                true => Result.RunCatching(() => transform(success)),
                _ => new Result<TNew, Exception>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the
        ///     encapsulated value if this instance represents success or the original encapsulated failure value if failure.
        /// </summary>
        /// <typeparam name="TNew">The type of the success value after transformation.</typeparam>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new <see cref="Result{TNew, Exception}" />.</returns>
        /// <remarks>
        ///     This function catches any <see cref="Exception" /> thrown by <paramref name="transform" /> function and
        ///     encapsulates it as a failure.
        ///     See <see cref="MapAsync" /> for an alternative that rethrows exceptions from `transform` function.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TNew, Exception>> MapCatchingAsync<TNew>(Func<TSuccess, Task<TNew>> transform) =>
            result.TryGetSuccess(out TSuccess? success) switch
            {
                true => Result.RunCatchingAsync(async () => await transform(success).ConfigureAwait(false)),
                _ => Task.FromResult(new Result<TNew, Exception>(result.Value))
            };
    }

    extension<TSuccess, TFailure, TRecover>(Result<TSuccess, TFailure> result) where TSuccess : TRecover
    {
        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     failure value if this instance represents failure or the original encapsulated value if success.
        /// </summary>
        /// <param name="transform">The recovery function.</param>
        /// <returns>A new <see cref="Result{TRecover, TFailure}" />.</returns>
        /// <remarks>
        ///     Note, that this function rethrows any exception thrown by <paramref name="transform" /> function. See
        ///     <see cref="RecoverCatching" /> for an alternative that encapsulates exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TRecover, TFailure> Recover(Func<TFailure, TRecover> transform) =>
            result.TryGetFailure(out TFailure? failure) switch
            {
                true => Result.Success<TRecover, TFailure>(transform(failure)),
                false => new Result<TRecover, TFailure>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     failure value if this instance represents failure or the original encapsulated value if success.
        /// </summary>
        /// <param name="transform">The recovery function.</param>
        /// <returns>A new <see cref="Result{TRecover, TFailure}" />.</returns>
        /// <remarks>
        ///     Note, that this function rethrows any exception thrown by <paramref name="transform" /> function. See
        ///     <see cref="RecoverCatchingAsync" /> for an alternative that encapsulates exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Result<TRecover, TFailure>> RecoverAsync(Func<TFailure, Task<TRecover>> transform) =>
            result.TryGetFailure(out TFailure? failure) switch
            {
                true => Result.Success<TRecover, TFailure>(await transform(failure).ConfigureAwait(false)),
                false => new Result<TRecover, TFailure>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     failure value if this instance represents failure or the original encapsulated value if success.
        /// </summary>
        /// <param name="transform">The recovery function.</param>
        /// <returns>A new <see cref="Result{TRecover, Exception}" />.</returns>
        /// <remarks>
        ///     This function catches any <see cref="Exception" /> thrown by <paramref name="transform" /> function and
        ///     encapsulates it as a failure. See <see cref="Recover" /> for an alternative that rethrows exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TRecover, Exception> RecoverCatching(Func<TFailure, TRecover> transform) =>
            result.TryGetFailure(out TFailure? failure) switch
            {
                true => Result.RunCatching(() => transform(failure)),
                false => new Result<TRecover, Exception>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     failure value if this instance represents failure or the original encapsulated value if success.
        /// </summary>
        /// <param name="transform">The recovery function.</param>
        /// <returns>A new <see cref="Result{TRecover, Exception}" />.</returns>
        /// <remarks>
        ///     This function catches any <see cref="Exception" /> thrown by <paramref name="transform" /> function and
        ///     encapsulates it as a failure. See <see cref="RecoverAsync" /> for an alternative that rethrows exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TRecover, Exception>> RecoverCatchingAsync(Func<TFailure, Task<TRecover>> transform) =>
            result.TryGetFailure(out TFailure? failure) switch
            {
                true => Result.RunCatchingAsync(async () => await transform(failure).ConfigureAwait(false)),
                false => Task.FromResult(new Result<TRecover, Exception>(result.Value))
            };

        /// <summary>
        ///     Returns the encapsulated value if this instance represents success or the <paramref name="defaultValue" /> if
        ///     failure.
        /// </summary>
        /// <param name="defaultValue">The value to return if the result is a failure.</param>
        /// <returns>The success value or <paramref name="defaultValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TRecover GetOrDefault(TRecover defaultValue) => result.TryGetSuccess(out TSuccess? success) switch
        {
            true => success,
            _ => defaultValue
        };

        /// <summary>
        ///     Returns the encapsulated value if this instance represents success or the result of
        ///     <paramref name="onFailure" /> function for the encapsulated failure value if failure.
        /// </summary>
        /// <param name="onFailure">The function to execute if the result is failure.</param>
        /// <returns>The success value or the result of <paramref name="onFailure" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TRecover GetOrElse(Func<TFailure, TRecover> onFailure) =>
            result.Unfold(out TSuccess? success, out TFailure? failure) switch
            {
                false => success,
                true => onFailure(failure)
            };

        /// <summary>
        ///     Returns the encapsulated value if this instance represents success or the result of
        ///     <paramref name="onFailure" /> function for the encapsulated failure value if failure.
        /// </summary>
        /// <param name="onFailure">The function to execute if the result is failure.</param>
        /// <returns>The success value or the result of <paramref name="onFailure" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TRecover> GetOrElseAsync(Func<TFailure, Task<TRecover>> onFailure) =>
            result.Unfold(out TSuccess? success, out TFailure? failure) switch
            {
                false => success,
                true => await onFailure(failure).ConfigureAwait(false)
            };
    }
}
