using System.Runtime.CompilerServices;

namespace Mjolnir.Extensions.Railway;

public static class ResultExtensions
{
    extension<TSuccess, TFailure>(Result<TSuccess, TFailure> result)
    {
        /// <summary>
        ///     Returns the result of <paramref name="onSuccess" /> for the encapsulated value if this instance represents
        ///     <see cref="Result{TSuccess,TFailure}.IsSuccess" /> or the result of <paramref name="onFailure" /> function for the
        ///     encapsulated <typeparamref name="TFailure" /> if it is <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the fold operation.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is success.</param>
        /// <param name="onFailure">The function to execute if the result is failure.</param>
        /// <returns>The result of either <paramref name="onSuccess" /> or <paramref name="onFailure" />.</returns>
        /// <remarks>
        ///     This function rethrows any exception thrown by <paramref name="onSuccess" /> or by
        ///     <paramref name="onFailure" /> function.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Fold<TResult>(
            Func<TSuccess, TResult> onSuccess,
            Func<TFailure, TResult> onFailure
        ) => result.Unfold(out TSuccess? success, out TFailure? failure) switch
        {
            false => onSuccess(success),
            true => onFailure(failure)
        };

    #region "peek" onto value/exception and pipe

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

    #endregion

    #region transformation

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     value if this instance represents <see cref="Result{TSuccess,TFailure}.IsSuccess" /> or the original encapsulated
        ///     failure value if it is <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <typeparam name="TResult">The type of the success value after transformation.</typeparam>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new <see cref="Result{TResult, TFailure}"/>.</returns>
        /// <remarks>
        ///     This function rethrows any exception thrown by <paramref name="transform" /> function. See
        ///     <see cref="MapCatching" /> for an alternative that encapsulates exceptions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, TFailure> Map<TResult>(Func<TSuccess, TResult> transform) =>
            result.TryGetSuccess(out TSuccess? success) switch
            {
                true => Result.Success<TResult, TFailure>(transform(success)),
                _ => new Result<TResult, TFailure>(result.Value)
            };

        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the
        ///     encapsulated value if this instance represents success or the original encapsulated failure value if failure.
        /// </summary>
        /// <typeparam name="TResult">The type of the success value after transformation.</typeparam>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new <see cref="Result{TResult, Exception}"/>.</returns>
        /// <remarks>
        ///     This function catches any <see cref="Exception" /> thrown by <paramref name="transform" /> function and encapsulates it as a failure.
        ///     See <see cref="Map" /> for an alternative that rethrows exceptions from `transform` function.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Result<TResult, Exception> MapCatching<TResult>(Func<TSuccess, TResult> transform) =>
            result.TryGetSuccess(out TSuccess? success) switch
            {
                true => Result.RunCatching(() => transform(success)),
                _ => new Result<TResult, Exception>(result.Value)
            };
    }

    extension<TSuccess, TFailure, TRecover>(Result<TSuccess, TFailure> result) where TSuccess : TRecover
    {
        /// <summary>
        ///     Returns the encapsulated result of the given <paramref name="transform" /> function applied to the encapsulated
        ///     failure value if this instance represents failure or the original encapsulated value if success.
        /// </summary>
        /// <param name="transform">The recovery function.</param>
        /// <returns>A new <see cref="Result{TRecover, TFailure}"/>.</returns>
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
        /// <returns>A new <see cref="Result{TRecover, Exception}"/>.</returns>
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

    #endregion

        /// <summary>
        ///     Returns the encapsulated value if this instance represents success or the <paramref name="defaultValue" /> if failure.
        /// </summary>
        /// <param name="defaultValue">The value to return if the result is a failure.</param>
        /// <returns>The success value or <paramref name="defaultValue"/>.</returns>
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
        /// <returns>The success value or the result of <paramref name="onFailure"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TRecover GetOrElse(Func<TFailure, TRecover> onFailure) =>
            result.Unfold(out TSuccess? success, out TFailure? failure) switch
            {
                false => success,
                true => onFailure(failure)
            };
    }
}
