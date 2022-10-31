using System;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using TerraDotnet.TerraLcd;

namespace TerraDotnet;

public static class TerraDotnetServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosDotnet<T>(this IServiceCollection services, string lcdClientBaseUrl)
    {
        services.AddTransient<CosmosTransactionEnumerator<T>>();
        services.InternalAddLcdClient<T>(lcdClientBaseUrl);
        services.AddTransient<TerraMessageParser>();
        
        
        return services;
    }


    public static IServiceCollection InternalAddLcdClient<T>(this IServiceCollection services, string lcdClientBaseUrl)
    {
        services.AddRefitClient<ICosmosLcdApiClient<T>>(serviceProvider =>
            {
                var settings = new RefitSettings();

                settings.ContentSerializer = new SystemTextJsonContentSerializer(TerraJsonSerializerOptions.GetThem());
                
                return settings;
            })
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(lcdClientBaseUrl);
            });

        return services;
    }
}