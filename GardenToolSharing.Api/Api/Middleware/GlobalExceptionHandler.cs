using GardenToolSharing.Api.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GardenToolSharing.Api.Api.Middleware;

/// <summary>Turns exceptions into RFC 7807 ProblemDetails. Never leaks stack traces.</summary>
public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        switch (exception)
        {
            case DomainValidationException validation:
                problem = new HttpValidationProblemDetails(validation.Errors)
                {
                    Status = validation.StatusCode,
                    Title = validation.Title
                };
                break;

            case AppException app:
                problem = new ProblemDetails
                {
                    Status = app.StatusCode,
                    Title = app.Title,
                    Detail = app.Message
                };
                break;

            // Unparseable JSON, wrong enum value, bad query string value, wrong content type...
            case BadHttpRequestException bad:
                problem = new ProblemDetails
                {
                    Status = bad.StatusCode,
                    Title = "The request could not be read.",
                    Detail = "Check the request body and query parameters."
                };
                break;

            default:
                logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred."
                };
                break;
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
