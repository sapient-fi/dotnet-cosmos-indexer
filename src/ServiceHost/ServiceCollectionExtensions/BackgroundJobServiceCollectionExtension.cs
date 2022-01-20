using System.Configuration;
using System.Reflection;
using Hangfire;
using Hangfire.InMemory;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.DAL;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using Pylonboard.ServiceHost.RecurringJobs;
using RapidCore.DependencyInjection;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.Data;
using ServiceStack.OrmLite;

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
        services.AddTransient<PsiPoolArbJob>();
        services.AddTransient<MaterializedViewRefresherJob>();
        services.AddTransient<FxRateDownloadJob>();

        services.AddTransient<CronjobManager>();
        services.AddTransient<RecurringJobManager>();
        return services;
    }
}