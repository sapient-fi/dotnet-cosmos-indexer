using NewRelic.Api.Agent;
using Pylonboard.ServiceHost.Endpoints.Types;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MineStakingStats;

public class MineStakingStatsService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MineStakingStatsService(
        IDbConnectionFactory connectionFactory
    )
    {
        _connectionFactory = connectionFactory;
    }

    [Trace]
    public async Task<MineStakingStatsGraph> GetItAsync(CancellationToken cancellationToken)
    {
        using var db = await _connectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var stakingSumPerDayResults = await db.SqlListAsync<TimeSeriesStatEntry>(@"
select SUM(amount) as Value, DATE(created_at) as At
from terra_mine_staking_entity
group by DATE(created_at)
order by DATE(created_at);
", token: cancellationToken);

        var stakingCumSumResults = await db.SqlListAsync<TimeSeriesStatEntry>(@"
with data as (
  select
    DATE(created_at) as date,
    sum(amount) as amount
  from terra_mine_staking_entity
  group by 1
)
select
  date as at,
  sum(amount) over (order by date asc rows between unbounded preceding and current row) as value
from data order by date;
", token: cancellationToken);

        var daysStakedBinnedResults = await db.SqlListAsync<DaysStakedStatEntry>(@"
select sum(count) as count, daysStakedBin from(
    select count(*) as count, (FLOOR(days_to_now/5) * 5) as daysStakedBin
    from v_wallet_first_stake
    group by days_to_now
    order by daysStakedBin) as inr
group by inr.daysStakedBin;
", token: cancellationToken);
        
        var numNewWalletsResults = await db.SqlListAsync<TimeSeriesStatEntry>(@"
select distinct count(sender) as value, DATE(""min"") as at
        from v_wallet_first_stake
        group by DATE(""min"")
        order by DATE(""min"");
", token: cancellationToken);
        
        return new MineStakingStatsGraph
        {
            StakedPerDay = stakingSumPerDayResults,
            StakedPerDayCumulative = stakingCumSumResults,
            DaysStakedBinned = daysStakedBinnedResults,
            NewWalletsPerDay = numNewWalletsResults
        };
    }
}