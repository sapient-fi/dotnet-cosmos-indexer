using Pylonboard.ServiceHost.Endpoints;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

public static class EndpointsServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointServices(this IServiceCollection services)
    {
        services.AddScoped<MineStakingStatsService>();

        return services;
    }
}