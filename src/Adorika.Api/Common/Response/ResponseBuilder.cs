using System.Buffers;
using System.Diagnostics;

using Adorika.Application.Common.Wrapper;

namespace Adorika.Api.Common.Wrapper;

// 4. THE HIGH-PERFORMANCE BUILDER
public ref struct ResponseBuilder<T>(Result<T> result, HttpContext httpContext)
{
    public readonly IResult WithHateoas(params LinkSpec[] specs)
    {
        var gen = httpContext.RequestServices.GetRequiredService<LinkGenerator>();

        // Use ArrayPool to avoid List allocation on the heap
        var pooledLinks = ArrayPool<ResultLink>.Shared.Rent(specs.Length + 1);
        int linkCount = 0;

        try
        {
            // Add Self Link (Standard path generation)
            pooledLinks[linkCount++] = ResultLink.Self(httpContext.Request.Path);

            foreach (var spec in specs)
            {
                // .NET LinkGenerator uses Spans internally for GetPathByName
                var path = gen.GetPathByName(httpContext, spec.RouteName, spec.Values);
                if (!string.IsNullOrEmpty(path))
                {
                    pooledLinks[linkCount++] = new ResultLink(
                        Href: path,
                        Rel: spec.Rel,
                        Method: spec.Method,
                        Title: spec.Title,
                        Type: spec.Type);
                }
            }

            // Copy to final list for JSON serialization
            var finalLinks = new List<ResultLink>(linkCount);
            for (int i = 0; i < linkCount; i++) finalLinks.Add(pooledLinks[i]);

            var payload = new ApiPayload<T>(
                Message: result.Message ?? (result.IsSuccessful ? "Success" : "Failure"),
                Data: result.Data,
                Errors: result.Errors,
                Links: finalLinks,
                Timestamp: DateTimeOffset.UtcNow,
                MachineName: Environment.MachineName,
                TraceId: Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
                RequestId: httpContext.TraceIdentifier
            );

            int statusCode = result.IsSuccessful
                ? (httpContext.Request.Method == "POST" ? 201 : 200)
                : (result.Errors?.Count > 0 ? result.Errors[0].StatusCode : 400);

            return Results.Json(payload, statusCode: statusCode);
        }
        finally
        {
            // Return memory to the pool immediately
            ArrayPool<ResultLink>.Shared.Return(pooledLinks);
        }
    }
}

public class ApiResultFilter<T>(HateoasRegistry registry) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var resultValue = await next(context);
        if (resultValue is Result<T> result)
        {
            var specs = registry.GetSpecs(result.Data);
            return result.ToResponse(context.HttpContext).WithHateoas(specs.ToArray());
        }
        return resultValue;
    }
}
