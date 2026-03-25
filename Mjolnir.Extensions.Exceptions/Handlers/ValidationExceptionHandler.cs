using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mjolnir.Extensions.Exceptions.Handlers;

/// <summary>
///     Global exception handler for <see cref="ValidationException" /> instances.
///     Converts validation exceptions to ProblemDetails responses with grouped validation errors.
/// </summary>
internal sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <summary>
    ///     Attempts to handle a <see cref="ValidationException" /> by converting it to a ProblemDetails response.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>
    ///     A task that returns <c>true</c> if the exception was handled as a validation exception,
    ///     <c>false</c> if the exception is not a <see cref="ValidationException" />.
    /// </returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not ValidationException validationException) return false;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        ProblemDetailsContext context = new()
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest
            }
        };

        Dictionary<string, List<string>> errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());

        context.ProblemDetails.Extensions.TryAdd("errors", errors);

        return await problemDetailsService.TryWriteAsync(context);
    }
}
