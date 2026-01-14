using System.Reflection;

using Adorika.Application.Common.Behaviour;

using FluentValidation;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

namespace Adorika.Application;

public static class ConfigureDependencies
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        // Register the ValidationBehavior as an open generic pipeline behavior
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // HTTP Context Accessor (required for handlers that need HTTP context)
        services.AddHttpContextAccessor();

        return services;
    }
}
