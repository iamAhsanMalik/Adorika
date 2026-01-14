namespace Adorika.Api.Endpoints;

public static class ConfigureEndpoints
{
    public static IEndpointRouteBuilder MapFeaturesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapInstallationEndpoints();
        return app;
    }
}
