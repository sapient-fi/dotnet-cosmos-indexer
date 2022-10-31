using System;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SapientFi.Infrastructure.Terra2;
using TerraDotnet.TerraLcd;
using Xunit;

namespace NarrowIntegrationTests.Oracles.TerraLcdClient;

public class TerraMoneyLcdApiClientTests
{
    private readonly ICosmosLcdApiClient<Terra2Marker> _client;

    public TerraMoneyLcdApiClientTests()
    {
        _client = RestService.For<ICosmosLcdApiClient<Terra2Marker>>(
            new HttpClient(new HttpClientHandler
            {
                // To route via Charles proxy for debugging use: 
                // Proxy = new WebProxy("http://localhost:8888"),
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
            })
            {   
                BaseAddress = new Uri("https://phoenix-lcd.sapient.fi"),
            });
    }

    [Fact]
    public async Task GetLatestBlock_works()
    {
        var response = await _client.GetLatestBlockAsync();

        var block = response.Content;
        Assert.NotNull(block);
        Assert.NotNull(block!.Block.Header);
        Assert.NotNull(block.Block.Header.Height);
        Assert.NotNull(block.BlockId);
    }
}