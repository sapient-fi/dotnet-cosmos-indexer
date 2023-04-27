using Microsoft.Extensions.DependencyInjection;

namespace SapientFi.Kernel.Serialization;

public static class SerializationServiceCollectionExtensions
{
    public static IServiceCollection AddSerialization(this IServiceCollection services)
    {
        services.AddTransient<Jason>();

        return services;
    }
}