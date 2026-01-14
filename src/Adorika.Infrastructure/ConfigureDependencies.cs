using Adorika.Application.Common.Interfaces.Persistence;
using Adorika.Domain.Entities.Identity;
using Adorika.Domain.Entities.MultiTenancy;
using Adorika.Infrastructure.Persistence;
using Adorika.Infrastructure.Services;

using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adorika.Infrastructure;

public static class ConfigureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenant configuration
        services.AddMultiTenant<AppTenantInfo>()
            .WithRouteStrategy("tenant", useTenantAmbientRouteValue: true)
            .WithHeaderStrategy("X-Tenant-Id")
            .WithConfigurationStore();

        // Database Context Services
        services.AddScoped<IPlatformDbContext, PlatformDbContext>();
        services.AddDbContext<PlatformDbContext>(ConfigureDbContext(true, configuration));

        // 2. Register TenantDbContext (WITH Finbuckle, tenant-scoped)
        services.AddScoped<ITenantDbContext, TenantDbContext>();
        services.AddDbContext<TenantDbContext>(ConfigureDbContext(false, configuration));

        // HTTP Context Accessor (required for CurrentUserService)
        services.AddHttpContextAccessor();


        services.AddScoped<IDatabaseUtility, DatabaseUtility>();
        services.AddScoped<ISystemInstallation, SystemInstallation>();

        // Password Hashing
        services.AddScoped<IPasswordHasher<PlatformUser>, PasswordHasher<PlatformUser>>();
        services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

        // HTTP Context Accessor (required for CurrentUserService)
        services.AddHttpContextAccessor();

        // Memory Cache
        services.AddMemoryCache();
        return services;
    }

    private static Action<DbContextOptionsBuilder> ConfigureDbContext(bool isPlatformDb, IConfiguration configuration)
    {
        return options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("adorika"), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("Adorika.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable(isPlatformDb ? "__EFMigrationsPlatform" : "__EFMigrationsTenant");
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(30);
            });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(true);
        };
    }
}
