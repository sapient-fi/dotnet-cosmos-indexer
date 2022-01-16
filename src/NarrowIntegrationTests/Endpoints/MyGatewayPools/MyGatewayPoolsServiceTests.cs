using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NarrowIntegrationTests.Endpoints.Arbitraging;
using Pylonboard.ServiceHost.Endpoints.MyGatewayPools;
using Xunit;

namespace NarrowIntegrationTests.Endpoints.MyGatewayPools;

public class MyGatewayPoolsServiceTests : IntegrationBaseTest
{
    [Fact]
    public async Task Works()
    {
        var service = Scope.ServiceProvider.GetRequiredService<MyGatewayPoolService>();

        var data = await service.GetMyGatewayPoolsAsync(
            "terra14qul6swv2p3vcfqk38fm8dvkezf0gj52m6a78k",
            CancellationToken.None
        );

        Assert.NotEmpty(data);
    }
}