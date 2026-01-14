using Adorika.Api.Common.Middleware;
using Adorika.Api.Endpoints;
using Adorika.ServiceDefaults;

namespace Adorika.Api.Common.Extensions;

public static class ConfigureRequestPipeline
{
    public static async Task<IApplicationBuilder> AddRequestPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseHsts();

        // configure security headers to protect site hijacking
        app.UseSecurityHeaders();

        // enable cors for frontend application
        app.UseCors();

        app.MapDefaultEndpoints();
        app.MapFeaturesEndpoints();

        return app;
    }
}
