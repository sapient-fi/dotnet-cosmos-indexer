using Pylonboard.ServiceHost.Endpoints;
using Pylonboard.ServiceHost.Endpoints.Arbitraging;
using Pylonboard.ServiceHost.Endpoints.FxRates;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;
using Pylonboard.ServiceHost.Endpoints.MyGatewayPools;
using ServiceStack.Caching;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

public static class EndpointsServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointServices(this IServiceCollection services)
    {
        services.AddTransient<MineStakingStatsService>();
        services.AddTransient<GatewayPoolStatsService>();
        services.AddTransient<MineRankingService>();
        services.AddTransient<MineWalletStatsService>();
        services.AddTransient<MineTreasuryService>();
        services.AddTransient<ArbitrageService>();
        services.AddTransient<MyGatewayPoolService>();
        services.AddTransient<FxRatesService>();

        services.AddSingleton<ICacheClient>(c => new MemoryCacheClient());

        return services;
    }
}