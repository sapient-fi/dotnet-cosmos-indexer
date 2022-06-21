using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SapientFi.ServiceHost.ServiceCollectionExtensions;

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

    public void Dispose()
    {
        ServiceProvider.Dispose();
        Scope.Dispose();
    }
}