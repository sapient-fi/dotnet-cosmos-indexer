using Pylonboard.ServiceHost.Endpoints.MineStakingStats;

namespace Pylonboard.ServiceHost.Endpoints;

public class Query
{
    public Task<MineStakingStatsGraph> GetMineStakingStats([Service] MineStakingStatsService service,
        CancellationToken cancellationToken) => service.GetItAsync(cancellationToken);
}