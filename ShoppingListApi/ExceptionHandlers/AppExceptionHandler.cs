using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ShoppingListApi.Configs;
using ShoppingListApi.Controllers;
using ShoppingListApi.Exceptions;

namespace ShoppingListApi.ExceptionHandlers;

public sealed class AppExceptionHandler(
    ILogger<AppExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private readonly ILogger<AppExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("Handling exception of type {ExceptionType} in {HandlerName}",
            exception is NumberedException
                ? exception.InnerException?.GetType().FullName
                : exception.GetType().FullName,
            nameof(AppExceptionHandler));

        var problemDetails = exception switch
        {
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested
                => new ProblemDetails()
                {
                    Status = 499,
                    Title = "Client Closed Request!",
                    Detail = "The client closed the request before the server could respond.",
                },
            OperationCanceledException
                => new ProblemDetails()
                {
                    Status = 503,
                    Title = "Application Closing!",
                    Detail =
                        "Due to the server shutting down, your request could not be processed. Please try again later",
                },
            FormatException
                => new ProblemDetails()
                {
                    Status = 400,
                    Title = "Validation Error",
                    Detail = "Your input data was provided in a wrong format.",
                },
            _ => new ProblemDetails()
            {
                Status = 500,
                Title = "Internal error!",
                Detail = "Due to an internal error, your request could not be processed.",
            },
        };

        if (exception is NumberedException nEx) problemDetails.Extensions.Add("errorNumber", nEx.ErrorNumber);

        var result = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext()
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        if (result is false)
        {
            httpContext.Response.StatusCode = problemDetails.Status ?? 500;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        return result;
    }
}