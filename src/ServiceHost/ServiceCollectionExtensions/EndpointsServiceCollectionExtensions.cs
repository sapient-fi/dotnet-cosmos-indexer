using Pylonboard.ServiceHost.Endpoints;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

public static class EndpointsServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointServices(this IServiceCollection services)
    {
        services.AddScoped<MineStakingStatsService>();
        services.AddScoped<GatewayPoolStatsService>();
        services.AddScoped<MineRankingService>();
        services.AddScoped<MineWalletStatsService>();
        services.AddScoped<MineTreasuryService>();

        return services;
    }
}