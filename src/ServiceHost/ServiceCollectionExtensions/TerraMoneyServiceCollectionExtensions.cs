using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.Kernel.IdGeneration;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.Oracles;
using Pylonboard.ServiceHost.Oracles.ArbNotifier;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra.LowLevel;
using Pylonboard.ServiceHost.Oracles.TerraFcd;
using Pylonboard.ServiceHost.TerraDataFetchers;
using Pylonboard.ServiceHost.TerraDataFetchers.Internal.PylonPools;
using RapidCore.Locking;
using Refit;
using Telegram.Bot;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

public static class TerraMoneyServiceCollectionExtensions
{
    public static IServiceCollection AddTerraMoney(this IServiceCollection services, IConfiguration configuration)
    {
        var config = new PylonboardConfig(configuration);
        services.AddSingleton<IEnabledServiceRolesConfig>(config);

        services.AddSingleton<IDistributedAppLockProvider>(c => new NoopDistributedAppLockProvider());
        services.AddSingleton<IdGenerator>(c => new IdGenerator(new IdGen.IdGenerator(0)));

        services.AddRefitClient<ITerraMoneyFcdApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fcd.terra.dev"));

        services.AddRefitClient<ITerraMoneyExchangeRateApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.coinhall.org"));
        services.AddScoped<TerraExchangeRateOracle>();

        // services.AddScoped<TerraMoneyBackgroundServiceWorker>();
        // services.AddHostedService<ScopedBackgroundService<TerraMoneyBackgroundServiceWorker>>();

        services.AddScoped<PsiPoolArbServiceWorker>();
        services.AddHostedService<ScopedBackgroundService<PsiPoolArbServiceWorker>>();

        services.AddTransient<TerraTransactionEnumerator>();

        services.AddTransient<MineBuybackDataFetcher>();
        services.AddTransient<MineStakingDataFetcher>();

        services.AddTransient<PylonPoolsDataFether>();
        services.AddTransient<LowLevelPoolFetcher>();

        services.AddScoped<ArbNotifier>();
        services.AddScoped<ITelegramBotClient>(c => new TelegramBotClient("5008024993:AAHbSuGs__0YvSxoKkMQ0qPlwwTi6Pj7CVI"));

        return services;
    }
}