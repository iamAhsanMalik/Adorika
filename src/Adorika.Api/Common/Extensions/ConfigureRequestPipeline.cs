using Adorika.Api.Common.Middleware;
using Adorika.ServiceDefaults;

namespace Adorika.Api.Common.Extensions;

public static class ConfigureRequestPipeline
{
    public static async Task<IApplicationBuilder> AddRequestPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHttpsRedirection();
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        // configure security headers to protect site hijacking
        app.UseSecurityHeaders();

        // enable cors for frontend application
        app.UseCors();

        // map default endpoints (health, alive, openapi, scalar)
        // This should be called after all middleware but can be before other endpoints
        app.MapDefaultEndpoints();

        return app;
    }
}
