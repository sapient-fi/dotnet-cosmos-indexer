using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SapientFi.Infrastructure.Cosmos;
using SapientFi.Infrastructure.Kujira.Indexers.Delegations;
using SapientFi.Infrastructure.Kujira.Storage;
using SapientFi.Infrastructure.Kujira.TransactionListener;
using TerraDotnet;

namespace SapientFi.Infrastructure.Kujira;

public static class KujiraServiceCollectionExtensions
{
    public static IServiceCollection AddKujiraStack(this IServiceCollection services, IConfiguration rawConfig)
    {
        var config = new KujiraTransactionListenerConfig(rawConfig);

        services.AddSingleton(config);
        services.AddTransient<KujiraRawRepository>();
        services.AddKujiraDelegationsIndexer();
        services.AddTransient<KujiraTransactionListenerHostedService>();
        services.AddTransient<CosmosFactory<KujiraRawTransactionEntity>>();
        services.AddTransient<KujiraTransactionEnumerator>();
        services.AddCosmosDotnet<KujiraMarker>(config.LcdUri());

        if (config.DoEnable())
        {
            services.AddHostedService<KujiraTransactionListenerHostedService>();
        }
        
        return services;
    }
}