using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SapientFi.Infrastructure.Terra2.Indexers.Delegations;
using SapientFi.Infrastructure.Terra2.Storage;
using SapientFi.Infrastructure.Terra2.TransactionListener;
using TerraDotnet;

namespace SapientFi.Infrastructure.Terra2;

public static class Terra2ServiceCollectionExtensions
{
    public static IServiceCollection AddTerra2Stack(this IServiceCollection services, IConfiguration rawConfig)
    {
        var config = new Terra2TransactionListenerConfig(rawConfig);

        services.AddSingleton(config);
        services.AddTransient<Terra2RawRepository>();
        services.AddTerra2DelegationsIndexer();
        services.AddTransient<Terra2TransactionListenerHostedService>();
        services.AddTransient<Terra2Factory>();
        services.AddTerraDotnet();

        if (config.DoEnable)
        {
            services.AddHostedService<Terra2TransactionListenerHostedService>();
        }
        
        return services;
    }
}