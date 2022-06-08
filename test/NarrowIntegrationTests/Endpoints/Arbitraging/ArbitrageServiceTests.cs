using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sapient.ServiceHost.Endpoints.Arbitraging;
using Xunit;

namespace NarrowIntegrationTests.Endpoints.Arbitraging;

public class ArbitrageServiceTests : IntegrationBaseTest
{
    public ArbitrageServiceTests()
    {
    }

    [Fact]
    public async Task ItWorks()
    {
        var service = Scope.ServiceProvider.GetRequiredService<ArbitrageService>();

        var data = await service.GetArbTimeSeriesForMarketAsync(ArbitrageMarket.Nexus, CancellationToken.None);
        
        Assert.NotEmpty(data);
    }

    [Fact]
    public async Task Get_trading_bands()
    {
        var service = Scope.ServiceProvider.GetRequiredService<ArbitrageService>();

        var data = await service.GetArbitrageTimeSeriesBandsAsync(
            ArbitrageMarket.Nexus, 
            CancellationToken.None
        );
        Assert.NotEmpty(data);
        Assert.InRange(data.Count, 1, 55);
    }
}