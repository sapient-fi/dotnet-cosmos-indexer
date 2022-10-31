using Microsoft.Extensions.DependencyInjection;
using SapientFi.Infrastructure.Kujira.Indexers.Delegations.Storage;

namespace SapientFi.Infrastructure.Kujira.Indexers.Delegations;

public static class KujiraDelegationsServiceCollectionExtensions
{
    public static IServiceCollection AddKujiraDelegationsIndexer(this IServiceCollection services)
    {
        services.AddTransient<KujiraDelegationIndexer>();
        services.AddTransient<KujiraDelegationsRepository>();
        
        return services;
    }
}