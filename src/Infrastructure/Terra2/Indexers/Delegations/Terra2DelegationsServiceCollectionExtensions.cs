using Microsoft.Extensions.DependencyInjection;
using SapientFi.Infrastructure.Indexing;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations;

public static class Terra2DelegationsServiceCollectionExtensions
{
    public static IServiceCollection AddTerra2DelegationsIndexer(this IServiceCollection services)
    {
        services.AddTransient<IIndexer, Terra2DelegationsIndexer>();
        
        return services;
    }
}