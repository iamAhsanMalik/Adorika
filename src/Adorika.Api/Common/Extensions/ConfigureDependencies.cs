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

        // Add CORS with more restrictive policy for production
        builder.Services.AddCors(builder.Configuration);

        return builder.Services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];

        return services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials() // Required for cookies
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        });
    }
}