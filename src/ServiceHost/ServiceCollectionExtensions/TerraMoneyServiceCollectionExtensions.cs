using RapidCore.Locking;
using Refit;
using SapientFi.Infrastructure.Hosting.BackgroundWorkers;
using SapientFi.Infrastructure.Oracles.ExchangeRates.Terra.LowLevel;
using SapientFi.Kernel.Config;
using SapientFi.Kernel.IdGeneration;
using SapientFi.ServiceHost.Config;
using TerraDotnet;
using TerraDotnet.TerraFcd;

namespace SapientFi.ServiceHost.ServiceCollectionExtensions;

public static class TerraMoneyServiceCollectionExtensions
{
    public static IServiceCollection AddTerraMoney(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new CosmosIndexerConfig(configuration);
        services.AddSingleton<IEnabledServiceRolesConfig>(config);
        services.AddSingleton<IGatewayPoolsConfig>(config);
        services.AddSingleton<ICorsConfig>(config);
        services.AddSingleton<IFeatureConfig>(config);

        services.AddSingleton<IDistributedAppLockProvider>(_ => new NoopDistributedAppLockProvider());
        services.AddSingleton(_ => new IdProvider(new IdGen.IdGenerator(0)));

        services.AddRefitClient<ITerraMoneyFcdApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fcd.terra.dev"));

        services.AddRefitClient<ITerraMoneyExchangeRateApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.coinhall.org"));
        
        services.AddScoped<CacheRefresherServiceWorker>();
        services.AddHostedService<ScopedBackgroundService<CacheRefresherServiceWorker>>();
        
        
        services.AddTransient<TerraTransactionEnumerator>();

        
        return services;
    }
}