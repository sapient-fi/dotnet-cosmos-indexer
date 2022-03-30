using MassTransit;
using NewRelic.Api.Agent;
using Pylonboard.Kernel;
using Pylonboard.Kernel.Contracts.Exchanges;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra.Views;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;

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

    [Trace]
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
                    case TerraPylonPoolOperation.Buy:
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
                    case TerraPylonPoolOperation.Sell:
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

    [Trace]
    private void FillOutDatesAndRewardDenom(MyGatewayPoolGraph graph)
    {
        switch (graph.FriendlyName)
        {
            case TerraPylonPoolFriendlyName.WhiteWhale1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 22, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1637672400 + 23328000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1637672400 + 46656000);
                graph.RewardDenominator = TerraDenominators.Whale;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.WHALE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.WhiteWhale2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 22, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1637672400 + 15552000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1637672400 + 31104000);
                graph.RewardDenominator = TerraDenominators.Whale;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.WHALE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.WhiteWhale3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 22, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1637672400 + 7776000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1637672400 + 15552000);
                graph.RewardDenominator = TerraDenominators.Whale;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.WHALE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Loop1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 13, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1628836200 + 23328000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1628836200 + 46656000);;
                graph.RewardDenominator = TerraDenominators.Loop;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.LOOP).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Loop2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 13, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1628836200 + 15552000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1628836200 + 31104000);
                graph.RewardDenominator = TerraDenominators.Loop;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.LOOP).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Loop3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 13, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1628836200 + 7776000);
                graph.WithdrawAt =  DateTimeOffset.FromUnixTimeSeconds(1628836200 + 15552000);
                graph.RewardDenominator = TerraDenominators.Loop;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.LOOP).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Orion:
            {
                graph.StartedAt = new DateTimeOffset(2021, 11, 1, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt =  DateTimeOffset.FromUnixTimeSeconds(1635858000 + 2592000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1635858000 + 31104000);
                graph.RewardDenominator = TerraDenominators.Orion;
                // While the ORION token contract is in fact registred with a multiplier/divisor of 100M the pylon contracts are doing some magic
                // when returning pending rewards. Since the FE is using this divisor to convert the pending rewards into "human readable form"
                // We have to return an incorrect divisor here, otherwise rewards are mis-represented.
                graph.RewardUAmountDivisor = 1_000_000m;
                break;
            }
            case TerraPylonPoolFriendlyName.Valkyrie1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 8, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1633737600 + 23328000);
                graph.WithdrawAt =  DateTimeOffset.FromUnixTimeSeconds(1633737600 + 46656000);
                graph.RewardDenominator = TerraDenominators.Vkr;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.VKR).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Valkyrie2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 8, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1633737600 + 15552000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1633737600 + 31104000);
                graph.RewardDenominator = TerraDenominators.Vkr;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.VKR).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Valkyrie3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 8, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1633737600 + 7776000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1633737600 + 15552000);
                graph.RewardDenominator = TerraDenominators.Vkr;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.VKR).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.TerraWorld1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1629603000 + 23328000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1629603000 + 46656000);
                graph.RewardDenominator = TerraDenominators.Twd;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.TWD).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.TerraWorld2:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1629603000 + 15552000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1629603000 + 31104000);
                graph.RewardDenominator = TerraDenominators.Twd;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.TWD).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.TerraWorld3:
            {
                graph.StartedAt = new DateTimeOffset(2021, 8, 22, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1629603000 + 7776000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1629603000 + 15552000);
                graph.RewardDenominator = TerraDenominators.Twd;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.TWD).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Mine1:
            {
                graph.StartedAt = new DateTimeOffset(2021, 07, 02, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1625194800 + 23328000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1625194800 + 46656000);
                graph.RewardDenominator = TerraDenominators.Mine;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.MINE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Mine2:
            {
                graph.RewardDenominator = TerraDenominators.Mine;
                graph.StartedAt = new DateTimeOffset(2021, 07, 02, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1625194800 + 15552000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1625194800 + 31104000);
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.MINE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Mine3:
            {
                graph.RewardDenominator = TerraDenominators.Mine;
                graph.StartedAt = new DateTimeOffset(2021, 07, 02, 03, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1625194800 + 7776000);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1625194800 + 15552000);
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.MINE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Nexus:
            {
                // 4'th december 2022
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1638622800);
                graph.ClaimAt = graph.StartedAt;
                // 2 years later
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1701694800);
                graph.RewardDenominator = TerraDenominators.Psi;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.PSI).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Glow1:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1639918800);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1663592400);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1687179600);
                graph.RewardDenominator = TerraDenominators.Glow;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.GLOW).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Glow2:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1639918800);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1655643600);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1671454800);
                graph.RewardDenominator = TerraDenominators.Glow;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.GLOW).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Glow3:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1639918800);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1647694800);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1655643600);
                graph.RewardDenominator = TerraDenominators.Glow;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.GLOW).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Sayve1:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1640091600);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1663765200);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1687352400);
                graph.RewardDenominator = TerraDenominators.Sayve;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.SAYVE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Sayve2:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1640091600);
                graph.ClaimAt =  DateTimeOffset.FromUnixTimeSeconds(1655816400);
                graph.WithdrawAt =  DateTimeOffset.FromUnixTimeSeconds(1671627600);
                graph.RewardDenominator = TerraDenominators.Sayve;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.SAYVE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Sayve3:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1640091600);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1647867600);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1655816400);
                graph.RewardDenominator = TerraDenominators.Sayve;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.SAYVE).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Xdefi1:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1640264400);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1661259600);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1703336400);
                graph.RewardDenominator = TerraDenominators.Xdefi;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.XDEFI).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Xdefi2:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1640264400);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1650718800);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1671800400);
                graph.RewardDenominator = TerraDenominators.Xdefi;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.XDEFI).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.Xdefi3:
            {
                graph.StartedAt = DateTimeOffset.FromUnixTimeSeconds(1640178000);
                graph.ClaimAt = DateTimeOffset.FromUnixTimeSeconds(1645621200);
                graph.WithdrawAt = DateTimeOffset.FromUnixTimeSeconds(1655989200);
                graph.RewardDenominator = TerraDenominators.Xdefi;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.XDEFI).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.DeviantsFactions:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 10, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt;
                graph.WithdrawAt = graph.StartedAt;
                graph.RewardDenominator = TerraDenominators.Ust;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.USD).Divisor;
                break;
            }
            case TerraPylonPoolFriendlyName.GalaticPunks:
            {
                graph.StartedAt = new DateTimeOffset(2021, 10, 27, 14, 00, 00, TimeSpan.Zero);
                graph.ClaimAt = graph.StartedAt;
                graph.WithdrawAt = graph.StartedAt;
                graph.RewardDenominator = TerraDenominators.Ust;
                graph.RewardUAmountDivisor = new TerraAmount("1", TerraTokenContracts.USD).Divisor;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [Trace]
    private async Task<List<MyGatewayPoolsView>> FetchGatewayDataFromDbAsync(string terraWallet,
        CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);
        var data = await db.SelectAsync<MyGatewayPoolsView>(
            q => q.Depositor == terraWallet,
            token: cancellationToken
        );

        return data;
    }

    [Trace]
    public async Task<List<MyGatewayPoolDetailsGraph>> GetGatewayPoolDetailsAsync(
        string terraWallet,
        string poolContractId,
        CancellationToken cancellationToken
    )
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(cancellationToken);

        var data = await db.SelectAsync<MyGatewayPoolDetailsGraph>(
            db.From<TerraPylonPoolEntity>()
                .Join<TerraRawTransactionEntity>()
                .Where(q =>
                    q.Depositor == terraWallet
                    && q.PoolContract == poolContractId
                )
                .OrderByDescending(q => q.CreatedAt)
                .Select<TerraPylonPoolEntity, TerraRawTransactionEntity>((p, t) =>
                    new
                    {
                        p.Operation,
                        p.CreatedAt,
                        p.Amount,
                        p.Denominator,
                        t.TxHash
                    })
            ,
            token: cancellationToken);

        return data;
    }
}