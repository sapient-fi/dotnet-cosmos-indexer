using NewRelic.Api.Agent;
using Pylonboard.Kernel.DAL.Entities.Terra.Views;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MineWalletStats;

public class MineWalletStatsService
{
    private readonly ILogger<MineWalletStatsService> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MineWalletStatsService(
        ILogger<MineWalletStatsService> logger,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
    }

    [Trace]
    public async Task<(List<MineWalletStatsGraph> results, int total)> GetItAsync(int? skip, int? take, string sortBy)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync();

        var total = await db.ScalarAsync<int>(
            @"
select count(distinct wallet)
from v_wallet_stake_sum_v2;
");

        var results = await db.SqlListAsync<MineWalletStatsGraph>(
            db.From<MineWalletStakeView>()
                .Skip(skip)
                .Take(take)
        );

        return (results, total);
    }
}