using System.Collections.ObjectModel;
using HotChocolate.Types.Pagination;
using NewRelic.Api.Agent;
using Pylonboard.ServiceHost.Endpoints.Arbitraging;
using Pylonboard.ServiceHost.Endpoints.FxRates;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineStakingStats;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;
using Pylonboard.ServiceHost.Endpoints.MineWalletStats;
using Pylonboard.ServiceHost.Endpoints.MyGatewayPools;
using Pylonboard.ServiceHost.Endpoints.MyPylonStake;
using Pylonboard.ServiceHost.Endpoints.Types;
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

    [Trace]
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

    [Trace]
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

    [Trace]
    [UseOffsetPaging(DefaultPageSize = 100, IncludeTotalCount = true, MaxPageSize = 200)]
    public async Task<CollectionSegment<MineWalletStatsGraph>> GetMineWalletStats(int? skip, int? take, string sortBy,
        [Service] ICacheClient cacheClient,
        [Service] MineWalletStatsService service
    )
    {
        var cacheKey = $"cache:mine-wallet-stats:{skip}-{take}-{sortBy}";
        var data = cacheClient.Get<DataAndTotalCache<List<MineWalletStatsGraph>>>(cacheKey);

        if (data == default)
        {
            var (results, total) = await service.GetItAsync(skip, take, sortBy);
            data = new DataAndTotalCache<List<MineWalletStatsGraph>>
            {
                Data = results,
                Total = total
            };
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        var pageInfo = new CollectionSegmentInfo(skip * take < data.Total, skip > 0);

        var collectionSegment = new CollectionSegment<MineWalletStatsGraph>(
            new ReadOnlyCollection<MineWalletStatsGraph>(data.Data),
            pageInfo,
            ct => new ValueTask<int>(data.Total));

        return collectionSegment;
    }

    [Trace]
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

    [Trace]
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
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"cache:mine-pool-stats:{gatewayIdentifier}:{skip}-{take}-{sortBy}";
        var data = cacheClient.Get<DataAndTotalCache<IList<GatewayPoolMineStakerStatsOverallGraph>>>(cacheKey);

        if (data == default)
        {
            var (results, total) =
                await service.GetMineStakerOverviewAsync(skip, take, gatewayIdentifier, cancellationToken);

            data = new DataAndTotalCache<IList<GatewayPoolMineStakerStatsOverallGraph>>
            {
                Data = results,
                Total = total,
            };

            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        
        var pageInfo = new CollectionSegmentInfo(skip * take < data.Total, skip > 0);

        var collectionSegment = new CollectionSegment<GatewayPoolMineStakerStatsOverallGraph>(
            new ReadOnlyCollection<GatewayPoolMineStakerStatsOverallGraph>(data.Data),
            pageInfo,
            ct => new ValueTask<int>(data.Total));

        return collectionSegment;
    }

    [Trace]
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
            data = await service.GetMineStakerRankingAsync(gatewayPoolIdentifier, cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        return data;
    }

    [Trace]
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
            data = await service.GetTotalValueStatsAsync(cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromHours(1));
        }

        return data;
    }

    [Trace]
    public async Task<ArbitrageGraph> GetArbitrageForMarket(
        [Service] ArbitrageService service,
        [Service] ICacheClient cacheClient,
        ArbitrageMarket market,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"cache:arbitrage:{market}";
        var data = cacheClient.Get<ArbitrageGraph>(cacheKey);
        if (data == default)
        {
            var results = await service.GetArbTimeSeriesForMarketAsync(market, cancellationToken);
            data = new ArbitrageGraph
            {
                Items = results
            };

            cacheClient.Set(cacheKey, data, TimeSpan.FromMinutes(65));
        }

        return data;
    }

    [Trace]
    public async Task<List<MyGatewayPoolGraph>> GetMyGatewayPools(
        string terraWallet,
        [Service] MyGatewayPoolService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"cache:mypools:{terraWallet}";
        var data = cacheClient.Get<List<MyGatewayPoolGraph>>(cacheKey);

        if (data == default)
        {
            var results = await service.GetMyGatewayPoolsAsync(terraWallet, cancellationToken);
            cacheClient.Set(cacheKey, results, TimeSpan.FromHours(1));
            data = results;
        }

        return data;
    }

    [Trace]
    public async Task<FxRateGraph> GetFxRates(
        FxRateQuery[] rates,
        [Service] FxRatesService fxRatesService,
        CancellationToken cancellationToken
    )
    {
        var results = await fxRatesService.ConvertEmAllAsync(rates, cancellationToken);

        return new FxRateGraph
        {
            Rates = results,
        };
    }

    [Trace]
    public async Task<List<MyGatewayPoolDetailsGraph>> GetMyGatewayPoolsDetails(
        string terraWallet,
        string poolContractId,
        [Service] MyGatewayPoolService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"cache:pools:{terraWallet}:{poolContractId}";
        var data = cacheClient.Get<List<MyGatewayPoolDetailsGraph>>(cacheKey);

        if (data == default)
        {
            data = await service.GetGatewayPoolDetailsAsync(terraWallet, poolContractId, cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromMinutes(60));
        }

        return data;
    }

    [Trace]
    public async Task<MyPylonStakeGraph> GetMyPylonStake(
        string terraWallet,
        [Service] MyPylonStakeService service,
        [Service] ICacheClient cacheClient,
        CancellationToken cancellationToken
    )
    {
        var cacheKey = $"cache:pylon-stake:{terraWallet}";
        var data = cacheClient.Get<MyPylonStakeGraph>(cacheKey);

        if (data == default)
        {
            data = await service.GetMyPylonStakeAsync(terraWallet, cancellationToken);
            cacheClient.Set(cacheKey, data, TimeSpan.FromMinutes(30));
        }

        return data;
    }
}