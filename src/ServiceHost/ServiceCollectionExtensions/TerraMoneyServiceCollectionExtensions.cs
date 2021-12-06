using Pylonboard.ServiceHost.Oracles.TerraFcd;
using Refit;

namespace Pylonboard.ServiceHost.Extensions;

public static class TerraMoneyServiceCollectionExtensions
{
    public static IServiceCollection AddTerraMoney(this IServiceCollection services, IConfiguration configuration)
    {
        // var terraMoneyConfig = new TerraMoneyConfig(configuration);
        // services.AddSingleton(terraMoneyConfig);
        // services.AddSingleton<ITerraMoneyConfig>(terraMoneyConfig);
        //     
        services.AddRefitClient<ITerraMoneyFcdApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fcd.terra.dev"));
            
        services.AddScoped<TerraMoneyBackgroundServiceWorker>();
        services.AddHostedService<ScopedBackgroundService<TerraMoneyBackgroundServiceWorker>>();
            
        services.AddTransient<TerraTransactionEnumerator>();
        services.AddTransient<MyTerraWalletDataFetcher>();
        services.AddTransient<AnchorContractHandler>();
            
        services.AddTransient<MineBuybackDataFetcher>();
        services.AddTransient<MineStakingDataFetcher>();
        services.AddTransient<TerraRewardsCalculator>();

        services.AddTransient<PylonPoolsDataFether>();
        services.AddTransient<LowLevelPoolFetcher>();
            
        return services;
    }
}