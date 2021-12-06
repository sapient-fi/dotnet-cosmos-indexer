using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.TerraDataFetchers;

namespace Pylonboard.ServiceHost.BackgroundServices;

public class TerraMoneyBackgroundServiceWorker : IScopedBackgroundServiceWorker
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
            _logger.LogInformation("Service role {Role} is not enabling, not starting", ServiceRoles.BACKGROUND_WORKER);
            return;
        }
            
        _logger.LogInformation("{TheThing} starting work", nameof(TerraMoneyBackgroundServiceWorker));

        do
        {
            await _mineStakingDataFetcher.FetchDataAsync(stoppingToken);
            await _mineBuybackDataFetcher.FetchDataAsync(stoppingToken);
            await _pylonPoolsDataFether.FetchDataAsync(stoppingToken);
                
            var sleepTime = TimeSpan.FromMinutes(10);
            _logger.LogDebug("Done for now, sleeping for {Sleep}", sleepTime.ToString("g"));
            await Task.Delay(sleepTime, stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}