using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Refit;
using TerraDotnet;
using TerraDotnet.TerraLcd;
using TerraDotnet.TerraLcd.Messages;
using Xunit;
using Xunit.Abstractions;

namespace NarrowIntegrationTests.Oracles.TerraLcdClient
{
    public class TerraMoneyLcdApiClientTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ITerraMoneyLcdApiClient _client;

        public TerraMoneyLcdApiClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _client = RestService.For<ITerraMoneyLcdApiClient>(
                new HttpClient(new HttpClientHandler
                {
                    // To route via Charles proxy for debugging use: 
                    // Proxy = new WebProxy("http://localhost:8888"),
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                })
                {   
                    BaseAddress = new Uri("https://lcd.terra.dev"),
                });
        }

        [Fact]
        public async Task GetLatestBlock_works()
        {
            var response = await _client.GetLatestBlockAsync();

            var block = response.Content;
            Assert.NotNull(block);
            Assert.NotNull(block.Block?.Header);
            Assert.NotNull(block.Block?.Header.Height);
            Assert.NotNull(block.BlockId);
        }

        [Fact]
        public async Task QueryContract_works()
        {
            var asJson = JsonSerializer.Serialize(new TerraClaimableRewardsRequest
            {
                ClaimableReward = new TerraClaimableRewardBody
                {
                    Owner = "terra14qul6swv2p3vcfqk38fm8dvkezf0gj52m6a78k",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            });
            
            var data = await _client.FetchPylonPoolDataAsync(TerraPylonGatewayContracts.MINE_1, asJson);

            Assert.NotNull(data);
            Assert.NotNull(data?.Height);
            Assert.NotNull(data?.Result?.Amount);
        }
    }

    
}