using Adorika.Application.Common.Wrapper;

namespace Adorika.Api.Common.Response;

// 3. THE REGISTRY (Remains Same)
public sealed class HateoasRegistry
{
    private readonly Dictionary<Type, object> _mappings = new();
    public void Register<T>(Func<T, IEnumerable<LinkSpec>> specFactory) => _mappings[typeof(T)] = specFactory;
    public IEnumerable<LinkSpec> GetSpecs<T>(T? data)
    {
        return data != null && _mappings.TryGetValue(typeof(T), out var factory)
            ? ((Func<T, IEnumerable<LinkSpec>>)factory)(data)
            : Enumerable.Empty<LinkSpec>();
    }
}

// 5. FILTER & EXTENSIONS (Remain Same)
public static class HateoasExtensions
{
    private static HateoasRegistry? _startupRegistry;

    public static void InitializeHateoas(this IServiceCollection services, HateoasRegistry registry)
    {
        services.AddSingleton(registry);
        _startupRegistry = registry;
    }

    public static ResponseBuilder<T> ToResponse<T>(this Result<T> result, HttpContext context)
        => new ResponseBuilder<T>(result, context);

    public static RouteHandlerBuilder WithLinks<T>(this RouteHandlerBuilder builder, Func<T, IEnumerable<LinkSpec>> specFactory)
    {
        _startupRegistry?.Register<T>(specFactory);
        return builder.AddEndpointFilter<ApiResultFilter<T>>();
    }
}
