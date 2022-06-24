using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SapientFi.ServiceHost.ServiceCollectionExtensions;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Xunit;

namespace NarrowIntegrationTests;

public abstract class IntegrationBaseTest : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IServiceScope Scope;

    protected IntegrationBaseTest()
    {
        var serviceCollection = new ServiceCollection();
        
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        serviceCollection.AddLogging();
        serviceCollection.AddDbStack(configBuilder.Build());
        serviceCollection.AddEndpointServices();
        
        // ReSharper disable once VirtualMemberCallInConstructor
        AddServices(serviceCollection);
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
        Scope = ServiceProvider.CreateScope();
    }

    public virtual void AddServices(IServiceCollection services)
    {
        
    }

    public virtual IDbConnectionFactory GetDbConnectionFactory()
    {
        return ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
        Scope.Dispose();
    }
    
    
    /// <summary>
    /// Ensure that the table for the entity exists and is empty.
    /// </summary>
    /// <typeparam name="TEntity">A database Entity</typeparam>
    protected async Task EnsureEmptyAsync<TEntity>()
    {
        using var db = await GetDbConnectionFactory().OpenDbConnectionAsync();
        db.CreateTableIfNotExists<TEntity>();
        await db.DeleteAllAsync<TEntity>();
    }
    
    
    protected async Task<TEntity> InsertAsync<TEntity>(TEntity entity)
    {
        using var db = await GetDbConnectionFactory().OpenDbConnectionAsync();
        await db.InsertAsync(entity);

        return entity;
    }
}