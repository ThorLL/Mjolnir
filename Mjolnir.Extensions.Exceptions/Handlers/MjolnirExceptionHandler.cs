using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Mjolnir.Extensions.Exceptions.Handlers;

/// <summary>
///     Global exception handler for <see cref="MjolnirException" /> instances.
///     Converts exceptions to ProblemDetails responses with appropriate HTTP status codes.
/// </summary>
internal sealed class MjolnirExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<MjolnirExceptionHandler> logger
) : IExceptionHandler
{
    /// <summary>
    ///     Attempts to handle a <see cref="MjolnirException" /> by converting it to a ProblemDetails response.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>
    ///     A task that returns <c>true</c> if the exception was handled, <c>false</c> if the exception
    ///     is not a <see cref="MjolnirException" /> or derived type.
    /// </returns>
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is AggregateException { InnerException: not null } ex) exception = ex.InnerException!;

        while (exception is MjolnirException { InnerException: MjolnirException innerEx }) exception = innerEx;

        string problemMessage = "Unhandled exception";
        Exception problemException = exception;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (exception is MjolnirException hammerEx)
        {
            problemException = hammerEx.InnerException ?? hammerEx;
            problemMessage = hammerEx.Message;
            httpContext.Response.StatusCode = hammerEx.StatusCode;
            if (hammerEx.StatusCode < 500)
                logger.LogInformation(exception, "Handled exception caught global handler");
            else
                logger.LogWarning(exception, "Handled exception caught global handler");
        }
        else
        {
            logger.LogError(exception, "Unhandled exception caught by global exception handler");
        }

        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = problemException,
            ProblemDetails = new ProblemDetails
            {
                Type = problemException.GetType().Name,
                Title = "An error occurred",
                Detail = problemMessage
            }
        });
    }
}
