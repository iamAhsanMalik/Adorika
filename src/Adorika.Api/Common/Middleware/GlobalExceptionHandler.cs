using System.Diagnostics;

using Adorika.Api.Common.Response;
using Adorika.Application.Common.Wrapper;

using Microsoft.AspNetCore.Diagnostics;

namespace Adorika.Api.Common.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment env) // Add environment check
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the full exception for internal tracking
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        // 2. Map Status Code & Friendly Message
        // You can expand this switch for specific business exceptions
        var (statusCode, message) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected server error occurred")
        };

        // 3. Environment-Aware Error Detail
        // In Development, we show the real error. In Production, we show a generic one.
        var displayMessage = env.IsDevelopment()
            ? exception.Message
            : message;

        // 4. Create the Standardized Error Object
        var error = new ResultError(
            Code: exception.GetType().Name,
            Message: displayMessage,
            StatusCode: statusCode);

        // 5. Build the Payload with HATEOAS
        // We add a 'self' link so the client knows exactly where the error happened
        var payload = new ApiPayload<object>(
            Message: "Internal Server Error",
            Data: null,
            Errors: new[] { error },
            Links: [ResultLink.Self(httpContext.Request.Path)], // Keep the HATEOAS consistent!
            Timestamp: DateTimeOffset.UtcNow,
            MachineName: Environment.MachineName,
            TraceId: Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
            RequestId: httpContext.TraceIdentifier
        );

        // 6. Write Response
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);

        return true;
    }
}
