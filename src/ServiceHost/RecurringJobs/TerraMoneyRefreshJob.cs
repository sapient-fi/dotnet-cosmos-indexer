using Polly;
using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.TerraDataFetchers;

namespace Pylonboard.ServiceHost.RecurringJobs;

public class TerraMoneyBackgroundServiceWorker
{
    private readonly ILogger<TerraMoneyBackgroundServiceWorker> _logger;
    private readonly MineStakingDataFetcher _mineStakingDataFetcher;
    private readonly MineBuybackDataFetcher _mineBuybackDataFetcher;
    private readonly PylonPoolsDataFether _pylonPoolsDataFether;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;

    public TerraMoneyBackgroundServiceWorker(
        ILogger<TerraMoneyBackgroundServiceWorker> logger,
        MineStakingDataFetcher mineStakingDataFetcher,
        MineBuybackDataFetcher mineBuybackDataFetcher,
        PylonPoolsDataFether pylonPoolsDataFether,
        IEnabledServiceRolesConfig serviceRolesConfig
    )
    {
        _logger = logger;
        _mineStakingDataFetcher = mineStakingDataFetcher;
        _mineBuybackDataFetcher = mineBuybackDataFetcher;
        _pylonPoolsDataFether = pylonPoolsDataFether;
        _serviceRolesConfig = serviceRolesConfig;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Service role {Role} is not enabled, will not start it", ServiceRoles.BACKGROUND_WORKER);
            return;
        }
            
        _logger.LogInformation("{TheThing} starting work", nameof(TerraMoneyBackgroundServiceWorker));
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
                TimeSpan.FromSeconds(40),
                TimeSpan.FromSeconds(80),
            }, (exception, waitTime, tryNumber, retryContext) =>
            {
                _logger.LogWarning(exception, "Got error while refreshing data, sleeping for {Sleep:T} before trying again. Retry number {Retry}", waitTime, tryNumber);
            });
        do
        {
            await retryPolicy.ExecuteAsync(async () => await _mineStakingDataFetcher.FetchDataAsync(stoppingToken));
            await retryPolicy.ExecuteAsync(async () => await _mineBuybackDataFetcher.FetchDataAsync(stoppingToken));
            await retryPolicy.ExecuteAsync(async () => await _pylonPoolsDataFether.FetchDataAsync(stoppingToken));
                
            var sleepTime = TimeSpan.FromMinutes(33);
            _logger.LogDebug("Done for now, sleeping for {Sleep}", sleepTime.ToString("g"));
            await Task.Delay(sleepTime, stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}