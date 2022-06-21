using System.Diagnostics;
using NewRelic.Api.Agent;
using SapientFi.Infrastructure.Hosting.BackgroundWorkers;
using SapientFi.Kernel.Config;
using SapientFi.ServiceHost.Endpoints;
using ServiceStack.Caching;

namespace SapientFi.ServiceHost;

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

                // TODO run GQL queries here that should be hydrated in the cache
                
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