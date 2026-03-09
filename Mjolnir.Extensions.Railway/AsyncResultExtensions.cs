using System.Runtime.CompilerServices;

namespace Mjolnir.Extensions.Railway;

public static partial class ResultExtensions
{
    public static Task<TSuccess> GetOrThrow<TSuccess, TFailure>(this Task<Result<TSuccess, TFailure>> result)
        where TFailure : Exception => result.Next(r => r.GetOrThrow());

    extension<TStart>(Task<TStart> start)
    {
        public async Task<TResult> Bind<TResult>(Func<TStart, Task<TResult>> next)
            => await next(await start.ConfigureAwait(false)).ConfigureAwait(false);

        public async Task<TResult> Next<TResult>(Func<TStart, TResult> next)
            => next(await start.ConfigureAwait(false));
    }

    extension<TSuccess, TFailure>(Task<Result<TSuccess, TFailure>> result)
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
        public Task<TNew> Fold<TNew>(
            Func<TSuccess, TNew> onSuccess,
            Func<TFailure, TNew> onFailure
        ) => result.Next(r => r.Fold(onSuccess, onFailure));

        /// <summary>
        ///     Performs the given <paramref name="action" /> on the encapsulated failure value if this instance
        ///     represents <see cref="Result{TSuccess,TFailure}.IsFailure" />.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The original <c>Result</c> unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TSuccess, TFailure>> OnFailure(Action<TFailure> action)
            => result.Next(r => r.OnFailure(action));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TSuccess, TFailure>> OnFailureAsync(Func<TFailure, Task> action) =>
            result.Bind(r => r.OnFailureAsync(action));

        /// <summary>
        ///     Performs the given <paramref name="action" /> on the encapsulated success value if this instance represents
        ///     <see cref="Result{TSuccess,TFailure}.IsSuccess" />.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The original <c>Result</c> unchanged.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TSuccess, TFailure>> OnSuccess(Action<TSuccess> action) =>
            result.Next(r => r.OnSuccess(action));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TSuccess, TFailure>> OnSuccessAsync(Func<TSuccess, Task> action) =>
            result.Bind(r => r.OnSuccessAsync(action));

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
        public Task<Result<TNew, TFailure>> Map<TNew>(Func<TSuccess, TNew> transform) =>
            result.Next(r => r.Map(transform));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TNew, TFailure>> MapAsync<TNew>(Func<TSuccess, Task<TNew>> transform) =>
            result.Bind(r => r.MapAsync(transform));

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
        public Task<Result<TNew, Exception>> MapCatching<TNew>(Func<TSuccess, TNew> transform) =>
            result.Next(r => r.MapCatching(transform));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TNew, Exception>> MapCatchingAsync<TNew>(Func<TSuccess, Task<TNew>> transform) =>
            result.Bind(r => r.MapCatchingAsync(transform));
    }

    extension<TSuccess, TFailure, TRecover>(Task<Result<TSuccess, TFailure>> result) where TSuccess : TRecover
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
        public Task<Result<TRecover, TFailure>> Recover(Func<TFailure, TRecover> transform) =>
            result.Next(r => r.Recover(transform));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TRecover, TFailure>> RecoverAsync(Func<TFailure, Task<TRecover>> transform) =>
            result.Bind(r => r.RecoverAsync(transform));

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
        public Task<Result<TRecover, Exception>> RecoverCatching(Func<TFailure, TRecover> transform) =>
            result.Next(r => r.RecoverCatching(transform));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Result<TRecover, Exception>> RecoverCatchingAsync(Func<TFailure, Task<TRecover>> transform) =>
            result.Bind(r => r.RecoverCatchingAsync(transform));

        /// <summary>
        ///     Returns the encapsulated value if this instance represents success or the <paramref name="defaultValue" /> if
        ///     failure.
        /// </summary>
        /// <param name="defaultValue">The value to return if the result is a failure.</param>
        /// <returns>The success value or <paramref name="defaultValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRecover> GetOrDefault(TRecover defaultValue) => result.Next(r => r.GetOrDefault(defaultValue));

        /// <summary>
        ///     Returns the encapsulated value if this instance represents success or the result of
        ///     <paramref name="onFailure" /> function for the encapsulated failure value if failure.
        /// </summary>
        /// <param name="onFailure">The function to execute if the result is failure.</param>
        /// <returns>The success value or the result of <paramref name="onFailure" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRecover> GetOrElse(Func<TFailure, TRecover> onFailure) =>
            result.Next(r => r.GetOrElse(onFailure));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<TRecover> GetOrElseAsync(Func<TFailure, Task<TRecover>> onFailure) =>
            result.Bind(r => r.GetOrElseAsync(onFailure));
    }
}
