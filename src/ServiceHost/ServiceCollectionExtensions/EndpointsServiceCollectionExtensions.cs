using Microsoft.Extensions.DependencyInjection;
using SapientFi.ServiceHost.Endpoints.FxRates;
using ServiceStack.Caching;

namespace SapientFi.ServiceHost.ServiceCollectionExtensions;

public static class EndpointsServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointServices(this IServiceCollection services)
    {
        services.AddTransient<FxRatesService>();

        services.AddSingleton<ICacheClient>(_ => new MemoryCacheClient());

        return services;
    }
}