using Hangfire;
using Hangfire.InMemory;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra;
using Pylonboard.Kernel.Config;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.RecurringJobs;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

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
        services.AddTransient<TerraBpsiDpLiquidityPoolRefreshJob>();

        services.AddTransient<CronjobManager>();
        services.AddTransient<RecurringJobManager>();

        services.AddSingleton<IDistributedLockProvider>(_ =>
            new PostgresDistributedSynchronizationProvider((new PylonboardConfig(configuration) as IDbConfig)
                .ConnectionString));

        return services;
    }
}