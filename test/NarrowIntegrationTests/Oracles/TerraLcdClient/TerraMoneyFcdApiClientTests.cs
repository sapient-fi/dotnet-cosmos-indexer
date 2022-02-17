using System;
using System.Net.Http;
using System.Threading.Tasks;
using Pylonboard.Infrastructure.Hosting.TerraDataFetchers;
using Refit;
using TerraDotnet.TerraFcd;
using Xunit;
using Xunit.Abstractions;

namespace NarrowIntegrationTests.Oracles.TerraLcdClient
{
    public class TerraMoneyFcdApiClientTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ITerraMoneyFcdApiClient _client;

        public TerraMoneyFcdApiClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _client = RestService.For<ITerraMoneyFcdApiClient>(
                new HttpClient(new HttpClientHandler
                {
                    // To route via Charles proxy for debugging use: 
                    // Proxy = new WebProxy("http://localhost:8888"),
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                })
                {   
                    BaseAddress = new Uri("https://fcd.terra.dev"),
                });
        }

        [Fact]
        public async Task List_txs_work()
        {
            var txs = await _client.ListTxesAsync(121079826, 100, TerraStakingContracts.MINE_STAKING_CONTRACT);
            
            Assert.NotEmpty(txs.Txs);
        }
    }

    
}