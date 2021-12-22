using System.Data;
using System.Text;
using NewRelic.Api.Agent;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.DAL.TerraMoney.Views;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using Pylonboard.ServiceHost.Endpoints.Types;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;

public class GatewayPoolStatsService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GatewayPoolStatsService(
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    [Trace]
    public async Task<GatewayPoolStatsGraph> GetItAsync(
        GatewayPoolIdentifier gatewayIdentifier,
        CancellationToken cancellationToken
    )
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);
        var friendlyNames = PoolIdentifierToFriendlyNames(gatewayIdentifier);

        var stats = await db.SingleAsync<GatewayPoolStatsOverallGraph>(db.From<TerraPylonPoolEntity>()
                .Select(entity => new
                {
                    TotalValueLocked = Sql.Sum("amount"),
                    MinDeposit = Sql.Min("amount"),
                    MaxDeposit = Sql.Max("amount"),
                    AverageDeposit = Sql.Avg("amount")
                })
                .Where(entity => Sql.In(entity.FriendlyName, friendlyNames) && Sql.In(entity.Operation,
                    new[] { TerraPylonPoolOperation.Deposit })),
            token: cancellationToken);

        stats.DepositPerWallet = await CreateDepositPerWalletStatsAsync(db, friendlyNames, cancellationToken);

        stats.DepositsOverTime = await db.SqlListAsync<TimeSeriesStatEntry>(
            db.From<TerraPylonPoolEntity>()
                .Select("SUM(amount) as Value, DATE(created_at) as At")
                .GroupBy("DATE(created_at)")
                .OrderBy("DATE(created_at)")
                .Where(entity => Sql.In(entity.FriendlyName, friendlyNames)
                                 && Sql.In(entity.Operation,
                                     new[] { TerraPylonPoolOperation.Deposit, TerraPylonPoolOperation.Withdraw }))
            , token: cancellationToken);

        return new GatewayPoolStatsGraph
        {
            Overall = stats,
        };
    }

    [Trace]
    public async Task<GatewayPoolTotalValueStatsGraph> GetTotalValueStatsAsync(CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        SqlExpression<TerraPylonPoolEntity> BaseQuery()
        {
            return db.From<TerraPylonPoolEntity>()
                .Select(entity => Sql.Sum(entity.Amount))
                .Where(entity => Sql.In(entity.Operation,
                    new[] { TerraPylonPoolOperation.Deposit, TerraPylonPoolOperation.Withdraw }));
        }

        var now = DateTimeOffset.UtcNow;
        var baseQuery = BaseQuery();
        var totalResults = await db.ScalarAsync<decimal>(baseQuery, cancellationToken);


        var last24HourResult =
            await db.ScalarAsync<decimal>(BaseQuery().Where(entity => entity.CreatedAt >= now.AddHours(-24)), token: cancellationToken);
        
        var last7DaysResult = 
            await db.ScalarAsync<decimal>(BaseQuery().Where(entity => entity.CreatedAt >= now.AddDays(-7)), token: cancellationToken);

        var last30DaysResult = 
            await db.ScalarAsync<decimal>(BaseQuery().Where(entity => entity.CreatedAt >= now.AddDays(-30)), token: cancellationToken);

        return new GatewayPoolTotalValueStatsGraph
        {
            TotalValueLocked = totalResults,
            ValueLocked24h = last24HourResult,
            ValueLocked7d = last7DaysResult,
            ValueLocked30d = last30DaysResult,
        };
    }

    [Trace]
    public async Task<(IList<GatewayPoolMineStakerStatsOverallGraph>, int)> GetMineStakerOverviewAsync(
        int? skip,
        int? take,
        GatewayPoolIdentifier gatewayIdentifier,
        CancellationToken cancellationToken
    )
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);
        var friendlyNames = PoolIdentifierToFriendlyNames(gatewayIdentifier);
        /*
         select sum(pool.amount) as deposit_amount,
       pool.depositor,
       sum(staking.amount) as staking_amount
from terra_pylon_pool_entity as pool
         left join terra_mine_staking_entity staking on staking.sender = pool.depositor

where pool.friendly_name in ('WhiteWhale1', 'WhiteWhale2', 'WhiteWhale3')
  and pool.operation in ('Deposit', 'Withdraw')
and staking.sender is null
GROUP BY pool.depositor
ORDER BY (sum(pool.amount)) DESC;
         */
        var baseQuery = db.From<TerraPylonPoolEntity>()
            .LeftJoin<TerraMineStakingEntity>((entity, stakingEntity) => entity.Depositor == stakingEntity.Sender)
            .Where(entity => Sql.In(entity.FriendlyName, friendlyNames) && Sql.In(entity.Operation,
                new[]
                {
                    TerraPylonPoolOperation.Deposit,
                    TerraPylonPoolOperation.Withdraw
                }));

        var total = await db.ScalarAsync<int>(baseQuery.Select(Sql.Count("*")), token: cancellationToken);

        var results = await db.SqlListAsync<GatewayPoolMineStakerStatsOverallGraph>(
            baseQuery
                .GroupBy(entity => entity.Depositor)
                .OrderByDescending(entity => Sql.Sum(entity.Amount))
                .Take(take)
                .Skip(skip)
                .Select<TerraPylonPoolEntity, TerraMineStakingEntity>((entity, stakingEntity) => new
                {
                    DepositAmount = Sql.Sum(entity.Amount),
                    StakingAmount = Sql.Sum(stakingEntity.Amount),
                    Depositor = entity.Depositor
                }), token: cancellationToken);

        return (results, total);
    }

    [Trace]
    private static async Task<List<WalletAndDepositEntry>> CreateDepositPerWalletStatsAsync(
        IDbConnection db,
        TerraPylonPoolFriendlyName[] friendlyNames,
        CancellationToken cancellationToken
    )
    {
        var rawData = await db.SqlListAsync<WalletAndDepositEntry>(
            db.From<TerraPylonPoolEntity>()
                .GroupBy(x => x.Depositor)
                .OrderByDescending(x => Sql.Sum(x.Amount))
                .Where(x => Sql.In(x.FriendlyName, friendlyNames)
                            && Sql.In(x.Operation,
                                new[] { TerraPylonPoolOperation.Deposit, TerraPylonPoolOperation.Withdraw }))
                .Select(x => new
                {
                    Wallet = x.Depositor,
                    Amount = Sql.Sum(x.Amount)
                }), token: cancellationToken);

        var wrangledReturnData = new List<WalletAndDepositEntry>(10);
        var others = new WalletAndDepositEntry
        {
            Amount = 0,
            Wallet = "others",
            InPercent = 0
        };
        // TODO is this faster to calculate in SQL?
        var totalSum = rawData.Sum(x => x.Amount);

        foreach (var item in rawData)
        {
            item.InPercent = item.Amount / totalSum * 100;

            if (item.InPercent < 1 && rawData.Count > 5)
            {
                others.Amount += item.Amount;
                others.InPercent += item.InPercent;
                continue;
            }

            wrangledReturnData.Add(item);
        }

        wrangledReturnData.Add(others);
        return wrangledReturnData;
    }

    [Trace]
    public async Task<GatewayPoolMineStakerRankGraph> GetMineStakerRankingAsync(
        GatewayPoolIdentifier gatewayIdentifier,
        CancellationToken cancellationToken
    )
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);
        var friendlyNames = PoolIdentifierToFriendlyNames(gatewayIdentifier);

        var fnQueryRankAsync = async (decimal minStake, decimal maxStake) =>
        {
            var result = await db.SingleAsync<GatewayPoolDepositorRankingView>(
                db.From<GatewayPoolDepositorRankingView>()
                    .Where(view => view.StakingAmount >= minStake
                                   && view.StakingAmount < maxStake)
                , token: cancellationToken);

            var data = new GatewayPoolMineStakerRankItemGraph
            {
                DepositAmountAvg = result.DepositAmountAvg,
                DepositAmountMax = result.DepositAmountMax,
                DepositAmountMedian = result.DepositAmountMedian,
                DepositAmountMin = result.DepositAmountMin,
                DepositAmountSum = result.DepositAmountSum,
                StakingLowerBound = minStake,
                StakingUpperBound = maxStake,
            };
            
            return data;
        };

        var result = new GatewayPoolMineStakerRankGraph
        {
            Tier1 = await fnQueryRankAsync(1, 1_000),
            Tier2 = await fnQueryRankAsync(1_000, 10_000),
            Tier3 = await fnQueryRankAsync(10_000, 100_000),
            Tier4 = await fnQueryRankAsync(100_000, 220_000),
            Tier5 = await fnQueryRankAsync(220_000, long.MaxValue),
        };

        return result;
    }

    [Trace]
    private static TerraPylonPoolFriendlyName[] PoolIdentifierToFriendlyNames(
        GatewayPoolIdentifier gatewayPoolIdentifier
    )
    {
        return gatewayPoolIdentifier switch
        {
            GatewayPoolIdentifier.WhiteWhale => new[]
            {
                TerraPylonPoolFriendlyName.WhiteWhale1,
                TerraPylonPoolFriendlyName.WhiteWhale2,
                TerraPylonPoolFriendlyName.WhiteWhale3
            },
            GatewayPoolIdentifier.Loop => new[]
            {
                TerraPylonPoolFriendlyName.Loop1,
                TerraPylonPoolFriendlyName.Loop2,
                TerraPylonPoolFriendlyName.Loop3,
            },
            GatewayPoolIdentifier.Orion => new[]
            {
                TerraPylonPoolFriendlyName.Orion,
            },
            GatewayPoolIdentifier.Valkyrie => new[]
            {
                TerraPylonPoolFriendlyName.Valkyrie1,
                TerraPylonPoolFriendlyName.Valkyrie2,
                TerraPylonPoolFriendlyName.Valkyrie3,
            },
            GatewayPoolIdentifier.TerraWorld => new[]
            {
                TerraPylonPoolFriendlyName.TerraWorld1,
                TerraPylonPoolFriendlyName.TerraWorld2,
                TerraPylonPoolFriendlyName.TerraWorld3,
            },
            GatewayPoolIdentifier.Mine => new[]
            {
                TerraPylonPoolFriendlyName.Mine1,
                TerraPylonPoolFriendlyName.Mine2,
                TerraPylonPoolFriendlyName.Mine3,
            },
            GatewayPoolIdentifier.Nexus => new[]
            {
                TerraPylonPoolFriendlyName.Nexus,
            },
            GatewayPoolIdentifier.Glow => new[]
            {
                TerraPylonPoolFriendlyName.Glow1,
                TerraPylonPoolFriendlyName.Glow2,
                TerraPylonPoolFriendlyName.Glow3,
            },
            GatewayPoolIdentifier.Sayve => new []
            {
                TerraPylonPoolFriendlyName.Sayve1,
                TerraPylonPoolFriendlyName.Sayve2,
                TerraPylonPoolFriendlyName.Sayve3,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(gatewayPoolIdentifier), gatewayPoolIdentifier, null)
        };
    }
}