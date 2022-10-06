using Hangfire;
using Hangfire.InMemory;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using SapientFi.Infrastructure.Oracles.ExchangeRates.Terra;
using SapientFi.Kernel.Config;
using SapientFi.ServiceHost.Config;
using SapientFi.ServiceHost.RecurringJobs;

namespace SapientFi.ServiceHost.ServiceCollectionExtensions;

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
        services.AddTransient<MaterializedViewRefresherJob>();

        services.AddTransient<CronjobManager>();
        services.AddTransient<RecurringJobManager>();

        services.AddSingleton<IDistributedLockProvider>(_ =>
            new PostgresDistributedSynchronizationProvider((new CosmosIndexerConfig(configuration) as IDbConfig)
                .ConnectionString));

        return services;
    }
}