using MassTransit;
using Pylonboard.Kernel;
using Pylonboard.ServiceHost.Consumers;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MyGatewayPools;

public class MyGatewayPoolService
{
    private readonly ILogger<MyGatewayPoolService> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IRequestClient<GetLatestExchangeRateRequest> _exchangeRateClient;

    public MyGatewayPoolService(
        ILogger<MyGatewayPoolService> logger,
        IDbConnectionFactory dbConnectionFactory,
        IRequestClient<GetLatestExchangeRateRequest> exchangeRateClient
    )
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
        _exchangeRateClient = exchangeRateClient;
    }

    public async Task<List<MyGatewayPoolGraph>> GetMyGatewayPoolsAsync(string terraWallet,
        CancellationToken cancellationToken)
    {
        var returnData = new List<MyGatewayPoolGraph>(4);
        var data = await FetchGatewayDataFromDbAsync(terraWallet, cancellationToken);

        var grouped = data.GroupBy(d => d.PoolContract);

        foreach (var item in grouped)
        {
            var poolContract = item.Key;
            var first = item.First();
            var graph = new MyGatewayPoolGraph
            {
                PoolContractAddress = poolContract,
                PoolIdentifier = FriendlyNameToPoolIdentifier(first.FriendlyName),
                FriendlyName = first.FriendlyName
            };

            FillOutDatesAndRewardDenom(graph);

            foreach (var action in item)
            {
                switch (action.Operation)
                {
                    case TerraPylonPoolOperation.Deposit:
                    {
                        graph.TotalDepositAmount = action.Amount;
                        break;
                    }
                    case TerraPylonPoolOperation.Claim:
                    {
                        graph.TotalClaimedAmount = action.Amount;
                        break;
                    }
                    case TerraPylonPoolOperation.Withdraw:
                        graph.TotalWithdrawnAmount = action.Amount;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var result = await _exchangeRateClient.GetResponse<GetLatestExchangeRateResult>(
                new GetLatestExchangeRateRequest
                {
                    FromDenominator = graph.RewardDenominator.ToUpperInvariant(),
                    ToDenominator = "UST"
                },
                cancellationToken
            );

            graph.TotalClaimedAmountInUst = result.Message.Value * graph.TotalClaimedAmount;
            graph.ClaimedAmountToUstMultiplier = result.Message.Value;
            
            returnData.Add(graph);
        }

        return returnData;
    }

    private GatewayPoolIdentifier FriendlyNameToPoolIdentifier(TerraPylonPoolFriendlyName firstFriendlyName)
    {
        switch (firstFriendlyName)
        {
            case TerraPylonPoolFriendlyName.WhiteWhale1:
            case TerraPylonPoolFriendlyName.WhiteWhale2:
            case TerraPylonPoolFriendlyName.WhiteWhale3:
                return GatewayPoolIdentifier.WhiteWhale;
            case TerraPylonPoolFriendlyName.Loop1:
            case TerraPylonPoolFriendlyName.Loop2:
            case TerraPylonPoolFriendlyName.Loop3:
                return GatewayPoolIdentifier.Loop;
            case TerraPylonPoolFriendlyName.Orion:
                return GatewayPoolIdentifier.Orion;
            case TerraPylonPoolFriendlyName.Valkyrie1:
            case TerraPylonPoolFriendlyName.Valkyrie2:
            case TerraPylonPoolFriendlyName.Valkyrie3:
                return GatewayPoolIdentifier.Valkyrie;
            case TerraPylonPoolFriendlyName.TerraWorld1:
            case TerraPylonPoolFriendlyName.TerraWorld2:
            case TerraPylonPoolFriendlyName.TerraWorld3:
                return GatewayPoolIdentifier.TerraWorld;
            case TerraPylonPoolFriendlyName.Mine1:
            case TerraPylonPoolFriendlyName.Mine2:
            case TerraPylonPoolFriendlyName.Mine3:
                return GatewayPoolIdentifier.Mine;
            case TerraPylonPoolFriendlyName.Nexus:
                return GatewayPoolIdentifier.Nexus;
            case TerraPylonPoolFriendlyName.Glow1:
            case TerraPylonPoolFriendlyName.Glow2:
            case TerraPylonPoolFriendlyName.Glow3:
                return GatewayPoolIdentifier.Glow;
            case TerraPylonPoolFriendlyName.Sayve1:
            case TerraPylonPoolFriendlyName.Sayve2:
            case TerraPylonPoolFriendlyName.Sayve3:
                return GatewayPoolIdentifier.Sayve;
            case TerraPylonPoolFriendlyName.Xdefi1:
            case TerraPylonPoolFriendlyName.Xdefi2:
            case TerraPylonPoolFriendlyName.Xdefi3:
                return GatewayPoolIdentifier.Xdefi;
            case TerraPylonPoolFriendlyName.DeviantsFactions:
                return GatewayPoolIdentifier.DeviantsFactions;
            case TerraPylonPoolFriendlyName.GalaticPunks:
                return GatewayPoolIdentifier.GalacticPunks;
            default:
                throw new ArgumentOutOfRangeException(nameof(firstFriendlyName), firstFriendlyName, null);
        }
    }

    private void FillOutDatesAndRewardDenom(MyGatewayPoolGraph graph)
    {
        switch (graph.FriendlyName)
        {
            case TerraPylonPoolFriendlyName.WhiteWhale1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 22, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Whale;
                break;
            }
            case TerraPylonPoolFriendlyName.WhiteWhale2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 22, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Whale;
                break;
            }
            case TerraPylonPoolFriendlyName.WhiteWhale3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 22, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Whale;
                break;
            }
            case TerraPylonPoolFriendlyName.Loop1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 13, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Loop;
                break;
            }
            case TerraPylonPoolFriendlyName.Loop2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 13, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Loop;
                break;
            }
            case TerraPylonPoolFriendlyName.Loop3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 13, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Loop;
                break;
            }
            case TerraPylonPoolFriendlyName.Orion:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 1, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(1);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Orion;
                break;
            }
            case TerraPylonPoolFriendlyName.Valkyrie1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 8, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Vkr;
                break;
            }
            case TerraPylonPoolFriendlyName.Valkyrie2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 8, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Vkr;
                break;
            }
            case TerraPylonPoolFriendlyName.Valkyrie3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 8, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Vkr;
                break;
            }
            case TerraPylonPoolFriendlyName.TerraWorld1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Twd;
                break;
            }
            case TerraPylonPoolFriendlyName.TerraWorld2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Twd;
                break;
            }
            case TerraPylonPoolFriendlyName.TerraWorld3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Twd;
                break;
            }
            case TerraPylonPoolFriendlyName.Mine1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 07, 02, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Mine;
                break;
            }
            case TerraPylonPoolFriendlyName.Mine2:
            {
                graph.RewardDenominator = TerraDenominators.Mine;
                graph.StartedAt = new DateTimeOffset(2021, 07, 02, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                break;
            }
            case TerraPylonPoolFriendlyName.Mine3:
            {
                graph.RewardDenominator = TerraDenominators.Mine;
                graph.StartedAt = new DateTimeOffset(2021, 07, 02, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                break;
            }
            case TerraPylonPoolFriendlyName.Nexus:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 5, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt;
                graph.WithdrawAt = graph.StartedAt.AddMonths(24);
                graph.RewardDenominator = TerraDenominators.Psi;
                break;
            }
            case TerraPylonPoolFriendlyName.Glow1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 19, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Glow;
                break;
            }
            case TerraPylonPoolFriendlyName.Glow2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 19, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Glow;
                break;
            }
            case TerraPylonPoolFriendlyName.Glow3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 19, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Glow;
                break;
            }
            case TerraPylonPoolFriendlyName.Sayve1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 21, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(9);
                graph.WithdrawAt = graph.StartedAt.AddMonths(18);
                graph.RewardDenominator = TerraDenominators.Sayve;
                break;
            }
            case TerraPylonPoolFriendlyName.Sayve2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 21, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(6);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Sayve;
                break;
            }
            case TerraPylonPoolFriendlyName.Sayve3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 21, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(3);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Sayve;
                break;
            }
            case TerraPylonPoolFriendlyName.Xdefi1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(8);
                graph.WithdrawAt = graph.StartedAt.AddMonths(24);
                graph.RewardDenominator = TerraDenominators.Xdefi;
                break;
            }
            case TerraPylonPoolFriendlyName.Xdefi2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(4);
                graph.WithdrawAt = graph.StartedAt.AddMonths(12);
                graph.RewardDenominator = TerraDenominators.Xdefi;
                break;
            }
            case TerraPylonPoolFriendlyName.Xdefi3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 12, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt.AddMonths(2);
                graph.WithdrawAt = graph.StartedAt.AddMonths(6);
                graph.RewardDenominator = TerraDenominators.Xdefi;
                break;
            }
            case TerraPylonPoolFriendlyName.DeviantsFactions:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 10, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt;
                graph.WithdrawAt = graph.StartedAt;
                graph.RewardDenominator = TerraDenominators.Ust;
                break;
            }
            case TerraPylonPoolFriendlyName.GalaticPunks:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 27, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt;
                graph.WithdrawAt = graph.StartedAt;
                graph.RewardDenominator = TerraDenominators.Ust;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<List<MyGatewayPoolResult>> FetchGatewayDataFromDbAsync(string terraWallet,
        CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        /*
         * select SUM(amount),
       denominator,
       operation,
       friendly_name,
       pool_contract
from terra_pylon_pool_entity
where depositor = 'terra14qul6swv2p3vcfqk38fm8dvkezf0gj52m6a78k'
group by friendly_name, operation, denominator, pool_contract
order by friendly_name;
         */
        var data = await db.SqlListAsync<MyGatewayPoolResult>(
            db.From<TerraPylonPoolEntity>()
                .Select(entity => new
                {
                    Amount = Sql.Sum(entity.Amount),
                    entity.Denominator,
                    entity.Operation,
                    entity.FriendlyName,
                    entity.PoolContract
                })
                .Where(entity => entity.Depositor == terraWallet)
                .GroupBy(entity => new
                {
                    entity.FriendlyName,
                    entity.Operation,
                    entity.Denominator,
                    entity.PoolContract
                })
                .OrderBy(entity => entity.FriendlyName), token: cancellationToken);

        return data;
    }
}