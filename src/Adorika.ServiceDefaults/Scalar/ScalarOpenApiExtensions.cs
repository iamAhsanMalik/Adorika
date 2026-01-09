using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Adorika.ServiceDefaults.Scalar;

internal static class ScalarOpenApiExtensions
{
    public static IServiceCollection AddScalarConfiguration(this IServiceCollection services)
    {
        return services.SetupScalarOpenApi();
    }

    private static IServiceCollection SetupScalarOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Paths?.Remove("/");
            document.Components ??= new();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Bearer token authentication. Use the /api/auth/login endpoint to obtain a token."
                }
            };
            return Task.CompletedTask;
        }));

        return services;
    }
}
