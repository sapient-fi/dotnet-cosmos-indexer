using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Pylonboard.ServiceHost.Oracles.TerraFcd;
using Pylonboard.ServiceHost.Oracles.TerraLcd.LowLevel;
using Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;
using Pylonboard.ServiceHost.TerraDataFetchers;
using Refit;
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