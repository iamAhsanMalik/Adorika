using Adorika.ServiceDefaults.Scalar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

namespace Adorika.ServiceDefaults;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string _healthEndpointPath = "/health";
    private const string _alivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(_healthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(_alivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        // configure OpenAPI for .NET 10 with document transformer
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = builder.Environment.ApplicationName;
                document.Info.Version = "v1.0";
                document.Info.Description = "API Documentation";

                return Task.CompletedTask;
            });
        });

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Map custom health check endpoints as minimal APIs to ensure OpenAPI visibility
            app.MapGet(_healthEndpointPath, HealthCheck())
            .WithName("HealthCheck")
            .WithTags("Infrastructure")
            .WithSummary("System Health Check")
            .WithDescription("Returns the health status of all registered health checks including database connections, external services, etc.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status503ServiceUnavailable);

            app.MapGet(_alivenessEndpointPath, AliveCheck())
            .WithName("AliveCheck")
            .WithTags("Infrastructure")
            .WithSummary("Liveness Probe")
            .WithDescription("Simple liveness probe to verify the application is running and responsive. Returns HTTP 200 if healthy.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status503ServiceUnavailable);

            // Configure Scalar UI
            app.UseScalarUI(app.Configuration);
        }

        return app;
    }

    private static Func<HealthCheckService, Task<IResult>> AliveCheck()
    {
        return async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync(predicate: check => check.Tags.Contains("live"));
            return report.Status == HealthStatus.Healthy
                ? Results.Ok(new { status = "Alive", message = "Application is running" })
                : Results.Json(new { status = "Unhealthy", message = "Application is not responsive" },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
        };
    }

    private static Func<HealthCheckService, Task<IResult>> HealthCheck()
    {
        return async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();
            return report.Status == HealthStatus.Healthy
                ? Results.Ok(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        duration = e.Value.Duration.TotalMilliseconds
                    })
                })
                : Results.Json(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        exception = e.Value.Exception?.Message,
                        duration = e.Value.Duration.TotalMilliseconds
                    })
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
        };
    }
}
