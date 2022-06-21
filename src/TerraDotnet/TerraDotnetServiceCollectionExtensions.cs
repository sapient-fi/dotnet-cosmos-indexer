using Microsoft.Extensions.DependencyInjection;
using Refit;
using TerraDotnet.TerraLcd;

namespace TerraDotnet;

public static class TerraDotnetServiceCollectionExtensions
{
    public static IServiceCollection AddTerraDotnet(this IServiceCollection services)
    {
        services.AddTransient<TerraTransactionEnumerator>();
        services.InternalAddLcdClient();
        
        
        return services;
    }


    public static IServiceCollection InternalAddLcdClient(this IServiceCollection services)
    {
        services.AddRefitClient<ITerraMoneyLcdApiClient>(serviceProvider =>
            {
                var settings = new RefitSettings();

                settings.ContentSerializer = new SystemTextJsonContentSerializer(TerraJsonSerializerOptions.GetThem());
                
                return settings;
            })
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("https://phoenix-lcd.terra.dev");
            });

        return services;
    }
}