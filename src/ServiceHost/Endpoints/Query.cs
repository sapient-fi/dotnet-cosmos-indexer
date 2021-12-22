using System.Collections.ObjectModel;
using HotChocolate.Types.Pagination;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineStakingStats;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;

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

    public Task<MineTreasuryGraph> GetMineTreasury([Service] MineTreasuryService service,
        CancellationToken cancellationToken) =>
        service.GetTreasuryOverviewAsync(cancellationToken
        );

    public Task<IEnumerable<MineBuybackGraph>> GetMineTreasuryBuybackByWallet(
        string wallet,
        [Service] MineTreasuryService service,
        CancellationToken cancellationToken) => service.GetBuybackByWalletAsync(wallet, cancellationToken);

    [UseOffsetPaging(DefaultPageSize = 100, IncludeTotalCount = true, MaxPageSize = 200)]
    public async Task<CollectionSegment<GatewayPoolMineStakerStatsOverallGraph>> GetGatewayPoolMineStakingStats(
        int? skip, 
        int? take,
        string sortBy,
        GatewayPoolIdentifier gatewayIdentifier,
        [Service] GatewayPoolStatsService service,
        CancellationToken cancellationToken
    )
    {
        {
            var (results, total) = await service.GetMineStakerOverviewAsync(skip, take, gatewayIdentifier, cancellationToken);

            var pageInfo = new CollectionSegmentInfo(skip * take < total, skip > 0);

            var collectionSegment = new CollectionSegment<GatewayPoolMineStakerStatsOverallGraph>(
                new ReadOnlyCollection<GatewayPoolMineStakerStatsOverallGraph>(results),
                pageInfo,
                ct => new ValueTask<int>(total));

            return collectionSegment;
        }
    }

    public async Task<GatewayPoolMineStakerRankGraph> GetGatewayPoolMineRanking(
        GatewayPoolIdentifier gatewayPoolIdentifier,
        [Service] GatewayPoolStatsService service,
        CancellationToken cancellationToken
    ) => await service.GetMineStakerRankingAsync(gatewayPoolIdentifier, cancellationToken);

    public async Task<GatewayPoolTotalValueStatsGraph> GetGatewayPoolTotalValues(
        [Service] GatewayPoolStatsService service,
        CancellationToken cancellationToken
    ) => await service.GetTotalValueStatsAsync(cancellationToken);
}