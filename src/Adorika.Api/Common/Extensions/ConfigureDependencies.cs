using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Adorika.Api.Common.Middleware;
using Adorika.Api.Common.Wrapper;
using Adorika.Application;
using Adorika.Infrastructure;
using Adorika.ServiceDefaults;

using Serilog;

namespace Adorika.Api.Common.Extensions;

public static class ConfigureDependencies
{
    public static IServiceCollection AddApiDependencies(this WebApplicationBuilder builder)
    {
        // configure Serilog from appsettings.json
        builder.Host.UseSerilog((hostingContext, config) => config.ReadFrom.Configuration(hostingContext.Configuration));

        // configure service defaults from .net aspire
        builder.AddServiceDefaults();

        // Infrastructure and Application dependencies
        builder.Services.AddApplicationDependencies();
        builder.Services.AddInfrastructureDependencies(builder.Configuration);

        // Add CORS with more restrictive policy for production
        builder.Services.AddCors(builder.Configuration);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        var registry = new HateoasRegistry();
        builder.Services.AddSingleton(registry);
        builder.Services.InitializeHateoas(registry);

        // Configure JSON serialization options
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.TypeInfoResolver = IgnoreEmptyCollection();
        });

        return builder.Services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];

        return services.AddCors(options => options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials() // Required for cookies
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            }));
    }
    private static DefaultJsonTypeInfoResolver IgnoreEmptyCollection()
    {
        return new DefaultJsonTypeInfoResolver
        {
            Modifiers = { static typeInfo => {
            if (typeInfo.Kind != JsonTypeInfoKind.Object) { return; }
                foreach (var property in typeInfo.Properties)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                    {
                        property.ShouldSerialize = (_, value) =>
                        value is IEnumerable collection && collection.GetEnumerator().MoveNext();
                    }
                }
            }}
        };
    }
}
