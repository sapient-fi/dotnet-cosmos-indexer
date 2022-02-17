using System.Diagnostics;
using NewRelic.Api.Agent;
using Pylonboard.Infrastructure.Hosting.BackgroundWorkers;
using Pylonboard.Kernel.Config;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.Endpoints;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;
using Pylonboard.ServiceHost.Endpoints.MineRankings;
using Pylonboard.ServiceHost.Endpoints.MineStakingStats;
using Pylonboard.ServiceHost.Endpoints.MineTreasury;
using Pylonboard.ServiceHost.Endpoints.MineWalletStats;
using ServiceStack.Caching;

namespace Pylonboard.ServiceHost;

public class CacheRefresherServiceWorker : IScopedBackgroundServiceWorker
{
    private readonly ILogger<CacheRefresherServiceWorker> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly IServiceProvider _serviceProvider;

    public CacheRefresherServiceWorker(
        ILogger<CacheRefresherServiceWorker> logger,
        IEnabledServiceRolesConfig serviceRolesConfig,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _serviceRolesConfig = serviceRolesConfig;
        _serviceProvider = serviceProvider;
    }

    [Trace]
    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.API))
        {
            _logger.LogInformation("API role not active, not starting cache refresher");
            return;
        }

        do
        {
            try
            {
                _logger.LogInformation("Starting background refresh of caches");
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                using var scope = _serviceProvider.CreateScope();
                var services = scope.ServiceProvider;
                var gql = new Query();
                var cacheClient = services.GetRequiredService<ICacheClient>();

                await gql.GetGatewayPoolTotalValues(services.GetRequiredService<GatewayPoolStatsService>(),
                    cacheClient,
                    stoppingToken);
                await gql.GetMineRankings(services.GetRequiredService<MineRankingService>(), cacheClient,
                    stoppingToken);
                await gql.GetMineTreasury(services.GetRequiredService<MineTreasuryService>(), cacheClient,
                    stoppingToken);
                foreach (var pool in (GatewayPoolIdentifier[])Enum.GetValues(typeof(GatewayPoolIdentifier)))
                {
                    await gql.GetGatewayPoolStats(pool, services.GetRequiredService<GatewayPoolStatsService>(),
                        cacheClient, stoppingToken);
                    await gql.GetGatewayPoolMineRanking(pool,
                        services.GetRequiredService<GatewayPoolStatsService>(),
                        cacheClient, stoppingToken);
                    await gql.GetGatewayPoolMineStakingStats(0, 10, "", pool,
                        services.GetRequiredService<GatewayPoolStatsService>(), cacheClient, stoppingToken);
                }

                await gql.GetMineStakingStats(services.GetRequiredService<MineStakingStatsService>(), cacheClient,
                    stoppingToken);
                await gql.GetMineWalletStats(0, 10, "", cacheClient,
                    services.GetRequiredService<MineWalletStatsService>());
                stopWatch.Stop();
                
                _logger.LogInformation("Background refresh of caches complete");
                NewRelic.Api.Agent.NewRelic.RecordMetric("cache-refresh-time-s", (float)stopWatch.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                NewRelic.Api.Agent.NewRelic.NoticeError(e);
                _logger.LogCritical(e, "Error while refreshing caches");
            }

            await Task.Delay(TimeSpan.FromMinutes(55), stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}