using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;

namespace Adorika.ServiceDefaults.Scalar;

public static class ScalarUIExtensions
{
    public static void UseScalarUI(this WebApplication app, IConfiguration configuration)
    {
        var scalarOptions = configuration.GetSection(ScalarUiSettings.SectionName).Get<ScalarUiSettings>();

        app.MapOpenApi();

        app.MapScalarApiReference(scalarOptions?.EndpointPrefix ?? "scalar", options =>
        {
            options.Title = scalarOptions?.Title;
            options.Theme = ScalarTheme.BluePlanet;
            options.ShowSidebar = true;
            options.DefaultOpenAllTags = scalarOptions?.ExpandAllTagsByDefault is true;
            options.HideModels = scalarOptions?.HideModelsSection is true;
            options.DarkMode = true;
            options.PersistentAuthentication = scalarOptions?.EnablePersistentAuthentication is true;
            options.DocumentDownloadType = DocumentDownloadType.Both;
            options.HideClientButton = true;
            options.ShowDeveloperTools = DeveloperToolsVisibility.Never;
            options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.CustomCss = """
            /* Custom CSS for Scalar UI */
                div.flex-1.min-w-0:has(a[href="https://www.scalar.com"]), .open-api-client-button {
                display: none !important;
                }
            """;
        });

        if (scalarOptions?.RedirectToDocumentation == true)
        {
            app.MapGet("/", () => Results.Redirect($"/scalar"))
                .ExcludeFromDescription();
        }
    }
}

