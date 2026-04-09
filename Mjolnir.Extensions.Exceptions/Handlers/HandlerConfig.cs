using Microsoft.AspNetCore.Mvc;

namespace Mjolnir.Extensions.Exceptions.Handlers;

public class HandlerConfig
{
    internal Dictionary<Type, Func<Exception, (ProblemDetails details, int statusCode)>> CustomExceptionHandlers { get; } = new();

    public void AddHandler<TException>(Func<TException, (ProblemDetails details, int statusCode)> customExceptionHandler) where TException : Exception
    {
        CustomExceptionHandlers.Add(typeof(TException), exception => customExceptionHandler((TException)exception));
    }
}
