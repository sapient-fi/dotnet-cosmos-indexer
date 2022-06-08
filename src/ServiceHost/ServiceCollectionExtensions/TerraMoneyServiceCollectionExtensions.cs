using RapidCore.Locking;
using Refit;
using Sapient.Infrastructure.Hosting.BackgroundWorkers;
using Sapient.Infrastructure.Hosting.TerraDataFetchers;
using Sapient.Infrastructure.Hosting.TerraDataFetchers.Internal.PylonPools;
using Sapient.Infrastructure.Oracles.ExchangeRates.Terra.LowLevel;
using Sapient.Kernel.Config;
using Sapient.Kernel.IdGeneration;
using Sapient.ServiceHost.Config;
using TerraDotnet;
using TerraDotnet.TerraFcd;

namespace Sapient.ServiceHost.ServiceCollectionExtensions;

public static class TerraMoneyServiceCollectionExtensions
{
    public static IServiceCollection AddTerraMoney(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new PylonboardConfig(configuration);
        services.AddSingleton<IEnabledServiceRolesConfig>(config);
        services.AddSingleton<IGatewayPoolsConfig>(config);
        services.AddSingleton<ICorsConfig>(config);
        services.AddSingleton<IFeatureConfig>(config);

        services.AddSingleton<IDistributedAppLockProvider>(c => new NoopDistributedAppLockProvider());
        services.AddSingleton<IdGenerator>(c => new IdGenerator(new IdGen.IdGenerator(0)));

        services.AddRefitClient<ITerraMoneyFcdApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fcd.terra.dev"));

        services.AddRefitClient<ITerraMoneyExchangeRateApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.coinhall.org"));
        
        services.AddScoped<CacheRefresherServiceWorker>();
        services.AddHostedService<ScopedBackgroundService<CacheRefresherServiceWorker>>();
        
        
        services.AddTransient<TerraTransactionEnumerator>();

        services.AddTransient<MineBuybackDataFetcher>();
        services.AddTransient<MineStakingDataFetcher>();
        services.AddTransient<MineTreasuryDataFetcher>();

        services.AddTransient<PylonPoolsDataFether>();
        services.AddTransient<LowLevelPoolFetcher>();
        
        return services;
    }
}