using System.Data;
using NewRelic.Api.Agent;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MineRankings;

public class MineRankingService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MineRankingService(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    [Trace]
    public async Task<MineRankingsGraph> GetItAsync(CancellationToken cancellationToken)
    {
        var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var stakePercentilesResult = await db.SingleAsync<MineStakingPercentileResult>(@"
select * from mv_wallet_stake_percentiles
", token: cancellationToken);


        return new MineRankingsGraph
        {
            Percentile99 = new MineRankingPercentileData
            {
                PercentileFloor = stakePercentilesResult.P99,
                WalletsIncluded =
                    await GetWalletsIncludedInPercentileAsync(stakePercentilesResult.P99, db, cancellationToken),
                AmountOfMine = await GetAmountOfMineInPercentileAsync(stakePercentilesResult.P99, db, cancellationToken)
            },
            Percentile95 = new MineRankingPercentileData
            {
                PercentileFloor = stakePercentilesResult.P95,
                WalletsIncluded =
                    await GetWalletsIncludedInPercentileAsync(stakePercentilesResult.P95, db, cancellationToken),
                AmountOfMine = await GetAmountOfMineInPercentileAsync(stakePercentilesResult.P95, db, cancellationToken)
            },
            Percentile90 = new MineRankingPercentileData
            {
                PercentileFloor = stakePercentilesResult.P90,
                WalletsIncluded =
                    await GetWalletsIncludedInPercentileAsync(stakePercentilesResult.P90, db, cancellationToken),
                AmountOfMine = await GetAmountOfMineInPercentileAsync(stakePercentilesResult.P90, db, cancellationToken)
            },
            Percentile75 = new MineRankingPercentileData
            {
                PercentileFloor = stakePercentilesResult.P75,
                WalletsIncluded =
                    await GetWalletsIncludedInPercentileAsync(stakePercentilesResult.P75, db, cancellationToken),
                AmountOfMine = await GetAmountOfMineInPercentileAsync(stakePercentilesResult.P75, db, cancellationToken)
            },
            Floor = new MineRankingPercentileData
            {
                PercentileFloor = 1,
                WalletsIncluded = await GetWalletsIncludedInPercentileAsync(1, db, cancellationToken),
                AmountOfMine = await GetAmountOfMineInPercentileAsync(1, db, cancellationToken)
            },
        };
    }

    private async Task<decimal> GetAmountOfMineInPercentileAsync(
        decimal percentileFloor,
        IDbConnection db,
        CancellationToken cancellationToken
    )
    {
        return await db.ScalarAsync<decimal>(@"
select sum(sum) from mv_wallet_stake_sum where sum >= @theFloor;
", new
        {
            theFloor = percentileFloor
        }, token: cancellationToken);
    }

    private async Task<int> GetWalletsIncludedInPercentileAsync(
        decimal percentileFloor,
        IDbConnection db,
        CancellationToken cancellationToken
    )
    {
        return await db.ScalarAsync<int>(@"
select count(*) from (
select wallet from mv_wallet_stake_sum where sum >= @theFloor
    ) as inr
", new
        {
            theFloor = percentileFloor
        }, token: cancellationToken);
    }
}

public record MineStakingPercentileResult
{
    public decimal P99 { get; set; }
    public decimal P95 { get; set; }
    public decimal P90 { get; set; }
    public decimal P75 { get; set; }
    public decimal Median { get; set; }
    public decimal Average { get; set; }
    public decimal Max { get; set; }
    public decimal Min { get; set; }
}