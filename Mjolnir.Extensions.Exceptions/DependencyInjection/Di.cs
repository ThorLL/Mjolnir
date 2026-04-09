using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mjolnir.Extensions.Exceptions.Handlers;

namespace Mjolnir.Extensions.Exceptions.DependencyInjection;

/// <summary>
///     Dependency injection extensions for configuring the Mjolnir exception handlers.
/// </summary>
public static class Di
{
    /// <summary>
    ///     Adds the Mjolnir exception handlers to the service collection.
    ///     Registers both <see cref="ValidationExceptionHandler" /> and <see cref="MjolnirExceptionHandler" />.
    /// </summary>
    /// <param name="services">The service collection to register handlers with.</param>
    /// <returns>The updated service collection for chaining.</returns>
    public static IServiceCollection AddMjolnirExceptionsHandler(
        this IServiceCollection services,
        Action<HandlerConfig>? configuration = null
    )
    {
        HandlerConfig config = new();
        configuration?.Invoke(config);
        services.AddSingleton(config);

        return services
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<MjolnirExceptionHandler>();
    }

    /// <summary>
    ///     Adds the global exception handler middleware to the application pipeline.
    ///     Must be called after <see cref="AddMjolnirExceptionsHandler" /> during service configuration.
    /// </summary>
    /// <param name="app">The application builder to add middleware to.</param>
    /// <returns>The updated application builder for chaining.</returns>
    public static IApplicationBuilder UseMjolnirExceptionHandler(this IApplicationBuilder app) =>
        app.UseExceptionHandler();
}
