using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pylonboard.ServiceHost.Endpoints.Arbitraging;
using Pylonboard.ServiceHost.ServiceCollectionExtensions;
using Xunit;

namespace NarrowIntegrationTests.Endpoints.Arbitraging;

public class ArbitrageServiceTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;

    public ArbitrageServiceTests()
    {
        var serviceCollection = new ServiceCollection();

        
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        serviceCollection.AddLogging();
        serviceCollection.AddDbStack(configBuilder.Build());
        serviceCollection.AddEndpointServices();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
    }

    [Fact]
    public async Task ItWorks()
    {
        var service = _scope.ServiceProvider.GetRequiredService<ArbitrageService>();

        var data = await service.GetArbTimeSeriesForMarketAsync(ArbitrageMarket.Nexus, CancellationToken.None);
        
        Assert.NotEmpty(data);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}