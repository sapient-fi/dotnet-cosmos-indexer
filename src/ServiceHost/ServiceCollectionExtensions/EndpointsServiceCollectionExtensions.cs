using Pylonboard.ServiceHost.Endpoints;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;
using ServiceStack.Caching;

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

        services.AddSingleton<ICacheClient>(c => new MemoryCacheClient());

        return services;
    }
}