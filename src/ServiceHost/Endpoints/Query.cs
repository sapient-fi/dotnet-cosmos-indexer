using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.MineStakingStats;

namespace Pylonboard.ServiceHost.Endpoints;

public class Query
{
    public Task<MineStakingStatsGraph> GetMineStakingStats(
        [Service] MineStakingStatsService service,
        CancellationToken cancellationToken
    ) => service.GetItAsync(cancellationToken);

    public Task<GatewayPoolStatsGraph> GetGatewayPoolStats(
        GatewayPoolIdentifier gatewayIdentifier,
        [Service] GatewayPoolStatsService service,
        CancellationToken cancellationToken
    ) => service.GetItAsync(gatewayIdentifier, cancellationToken);
}