using System.Collections.ObjectModel;
using HotChocolate.Types.Pagination;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
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

    public Task<MineRankingsGraph> GetMineRankings(
        [Service] MineRankingService service,
        CancellationToken cancellationToken
    ) => service.GetItAsync(cancellationToken);

    [UseOffsetPaging(DefaultPageSize = 100, IncludeTotalCount = true, MaxPageSize = 200)]
    public async Task<CollectionSegment<MineWalletStatsGraph>> GetMineWalletStats(int? skip, int? take, string sortBy,
        [Service] MineWalletStatsService service)
    {
        var (results, total) = await service.GetItAsync(skip, take, sortBy);
        
        var pageInfo = new CollectionSegmentInfo(skip * take < total, skip > 0);

        var collectionSegment = new CollectionSegment<MineWalletStatsGraph>(
            new ReadOnlyCollection<MineWalletStatsGraph>(results),
            pageInfo,
            ct => new ValueTask<int>(total));

        return collectionSegment;
    } 
}