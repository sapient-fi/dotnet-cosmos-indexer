using Sapient.ServiceHost.Endpoints.Arbitraging;
using Sapient.ServiceHost.Endpoints.FxRates;
using Sapient.ServiceHost.Endpoints.GatewayPoolStats;
using Sapient.ServiceHost.Endpoints.MineRankings;
using Sapient.ServiceHost.Endpoints.MineStakingStats;
using Sapient.ServiceHost.Endpoints.MineTreasury;
using Sapient.ServiceHost.Endpoints.MineWalletStats;
using Sapient.ServiceHost.Endpoints.MyGatewayPools;
using Sapient.ServiceHost.Endpoints.MyPylonStake;
using ServiceStack.Caching;

namespace Sapient.ServiceHost.ServiceCollectionExtensions;

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
        services.AddTransient<MyPylonStakeService>();

        services.AddSingleton<ICacheClient>(c => new MemoryCacheClient());

        return services;
    }
}