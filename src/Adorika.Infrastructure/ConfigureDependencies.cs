using Adorika.Domain.Entities.MultiTenancy;
using Adorika.Infrastructure.Persistence;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adorika.Infrastructure;

public static class ConfigureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMultiTenant<AppTenantInfo>()
            .WithRouteStrategy("tenant", useTenantAmbientRouteValue: true)
            .WithHeaderStrategy("X-Tenant-Id")
            .WithConfigurationStore();

        var configureDbContext = ConfigureDbContext(configuration);

        services.AddDbContext<AppDbContext>(options =>
        {
            configureDbContext(options);
        });

        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        return services;
    }

    public static async Task<WebApplication> UseInfrastructureMiddlewares(this WebApplication builder)
    {
        using (var scope = builder.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
            await initializer.InitializeAsync();
        }
        return builder;
    }

    private static Action<DbContextOptionsBuilder> ConfigureDbContext(IConfiguration configuration)
    {
        Action<DbContextOptionsBuilder> configureDbContext = options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("adorika"), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

            // Enable detailed errors in development
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(true);
        };
        return configureDbContext;
    }
}