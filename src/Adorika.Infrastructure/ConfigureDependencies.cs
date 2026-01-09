using Microsoft.Extensions.DependencyInjection;

namespace Adorika.Infrastructure;

public static class ConfigureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        return services;
    }
}