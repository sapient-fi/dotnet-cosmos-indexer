using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Refit;
using ServiceStack.Data;
using TerraDotnet.TerraLcd;
using TerraDotnet.TerraLcd.Messages;

namespace Pylonboard.Infrastructure.Oracles;

public class TerraMoneyLcdOracle
{
    private readonly ILogger<TerraMoneyLcdOracle> _logger;
    private readonly ITerraMoneyLcdApiClient _lowLevelClient;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public TerraMoneyLcdOracle(
        ILogger<TerraMoneyLcdOracle> logger,
        ITerraMoneyLcdApiClient lowLevelClient,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _logger = logger;
        _lowLevelClient = lowLevelClient;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task GetSinglePylonPoolRewardsAsync(string wallet, string gatewayPoolContract,
        CancellationToken cancellationToken)
    {
        // TODO check database cache

        var asJson = JsonSerializer.Serialize(new TerraClaimableRewardsRequest
        {
            ClaimableReward = new TerraClaimableRewardBody
            {
                Owner = wallet,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }
        });
        var result = await Policy
            .Handle<ApiException>(exception => exception.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(10, retryCounter => TimeSpan.FromMilliseconds(Math.Pow(10, retryCounter)),
                (exception, span) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Handling retry while fetching pylon pool rewards, waiting {Time:c}", span
                    );
                })
            .ExecuteAsync(async () => await _lowLevelClient.FetchPylonPoolDataAsync(
                gatewayPoolContract,
                asJson
            ));
        
        // TODO save in database cache
    }
}