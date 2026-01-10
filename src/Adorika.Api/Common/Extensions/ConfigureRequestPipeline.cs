using Adorika.Api.Common.Middleware;
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

        // map default endpoints (health, alive, openapi, scalar)
        // This should be called after all middleware but can be before other endpoints
        app.MapGet("/crash", () =>
        {
            Console.WriteLine("CRASH HIT!"); // Look at your console output
            throw new Exception("Brutal Crash!");
        });
        app.MapDefaultEndpoints();

        return app;
    }
}
