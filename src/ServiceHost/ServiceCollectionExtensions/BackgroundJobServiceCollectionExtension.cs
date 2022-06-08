using Hangfire;
using Hangfire.InMemory;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using Sapient.Infrastructure.Oracles.ExchangeRates.Terra;
using Sapient.Kernel.Config;
using Sapient.ServiceHost.Config;
using Sapient.ServiceHost.RecurringJobs;

namespace Sapient.ServiceHost.ServiceCollectionExtensions;

public static class BackgroundJobServiceCollectionExtension
{
    public static IServiceCollection AddBackgroundJobStack(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Hangfire services.
        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage(new InMemoryStorageOptions
            {
                DisableJobSerialization = true,
            }));

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        // Add our jobs
        services.AddTransient<TerraExchangeRateOracle>();
        services.AddTransient<TerraMoneyRefreshJob>();
        services.AddSingleton<PsiPoolArbJob>();
        services.AddTransient<MaterializedViewRefresherJob>();
        services.AddTransient<FxRateDownloadJob>();
        services.AddTransient<TerraBpsiDpLiquidityPoolTradesRefreshJob>();

        services.AddTransient<CronjobManager>();
        services.AddTransient<RecurringJobManager>();

        services.AddSingleton<IDistributedLockProvider>(_ =>
            new PostgresDistributedSynchronizationProvider((new PylonboardConfig(configuration) as IDbConfig)
                .ConnectionString));

        return services;
    }
}