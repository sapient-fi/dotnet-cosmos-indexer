using System.Collections.ObjectModel;
using HotChocolate.Types.Pagination;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineStakingStats;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;
using ServiceStack.Caching;

namespace Pylonboard.ServiceHost.Endpoints;

public class Query
{
    public async Task<MineStakingStatsGraph> GetMineStakingStats(
        [Service] MineStakingStatsService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = "cache:mine-staking-stats";
        var cached = cacheClient.Get<MineStakingStatsGraph>(cacheKey);
        if (cached != default)
        {
            return cached;
        }
        
        var data = await service.GetItAsync(cancellationToken);
        cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        return data;
    }

    public async Task<GatewayPoolStatsGraph> GetGatewayPoolStats(
        GatewayPoolIdentifier gatewayIdentifier,
        [Service] GatewayPoolStatsService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"cache:gateway-pool-stats:{gatewayIdentifier}";
        var data = cacheClient.Get<GatewayPoolStatsGraph>(cacheKey);

        if (data == default)
        {
            data = await service.GetItAsync(gatewayIdentifier, cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        return data;
    }

    public async Task<MineRankingsGraph> GetMineRankings(
        [Service] MineRankingService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = "cache:mine-rankings";
        var data = cacheClient.Get<MineRankingsGraph>(cacheKey);
        if (data == default)
        {
            data = await service.GetItAsync(cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }
        return data;
    }

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

    public async Task<MineTreasuryGraph> GetMineTreasury(
        [Service] MineTreasuryService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = "cache:mine-treasury";
        var data = cacheClient.Get<MineTreasuryGraph>(cacheKey);

        if (data == default)
        {
            data = await service.GetTreasuryOverviewAsync(cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        return data;
    }
      

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
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"cache:gw-pool-mine-rankings:{gatewayPoolIdentifier}";
        var data = cacheClient.Get<GatewayPoolMineStakerRankGraph>(cacheKey);
        if (data == default)
        {
            data =  await service.GetMineStakerRankingAsync(gatewayPoolIdentifier, cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        return data;
    }

    public async Task<GatewayPoolTotalValueStatsGraph> GetGatewayPoolTotalValues(
        [Service] GatewayPoolStatsService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = "cache:gateway-pool-tvl";
        var data = cacheClient.Get<GatewayPoolTotalValueStatsGraph>(cacheKey);

        if (data == default)
        {
            data= await service.GetTotalValueStatsAsync(cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        return data;
    }
}