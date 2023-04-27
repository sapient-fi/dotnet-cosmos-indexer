using Microsoft.Extensions.DependencyInjection;
using SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations;

public static class Terra2DelegationsServiceCollectionExtensions
{
    public static IServiceCollection AddTerra2DelegationsIndexer(this IServiceCollection services)
    {
        services.AddTransient<Terra2DelegationIndexer>();
        services.AddTransient<Terra2DelegationsRepository>();
        
        return services;
    }
}