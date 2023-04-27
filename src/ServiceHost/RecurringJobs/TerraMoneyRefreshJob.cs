using System;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Polly;
using SapientFi.Kernel.Config;

namespace SapientFi.ServiceHost.RecurringJobs;

public class TerraMoneyRefreshJob
{
    private readonly ILogger<TerraMoneyRefreshJob> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly IDistributedLockProvider _lockProvider;

    public TerraMoneyRefreshJob(
        ILogger<TerraMoneyRefreshJob> logger,
        IEnabledServiceRolesConfig serviceRolesConfig,
        IDistributedLockProvider lockProvider
    )
    {
        _logger = logger;
        _serviceRolesConfig = serviceRolesConfig;
        _lockProvider = lockProvider;
    }

    public async Task DoWorkAsync(
        CancellationToken stoppingToken,
        bool gatewayPoolfullResync = false,
        bool mineStakingFullResync = false,
        bool mineBuybackFullResync = false
    )
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Service role {Role} is not enabled, will not start it",
                ServiceRoles.BACKGROUND_WORKER);
            return;
        }

        await using var theLock = await _lockProvider.TryAcquireLockAsync("locks:job:terra-refresh", TimeSpan.Zero,
            cancellationToken: stoppingToken);
        if (theLock == default)
        {
            // the lock is a null instance meaning that we FAILED to acquire it... Abort basically
            _logger.LogWarning("Another terra refresh job is holding the lock, aborting");
            return;
        }

        _logger.LogInformation("{TheThing} starting work", nameof(TerraMoneyRefreshJob));
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(40),
                    TimeSpan.FromSeconds(80),
                },
                (exception, waitTime, tryNumber, _) =>
                {
                    _logger.LogWarning(exception,
                        "Got error while refreshing data, sleeping for {Sleep:T} before trying again. Retry number {Retry}",
                        waitTime, tryNumber);
                });

        // await retryPolicy.ExecuteAsync(async () => await _mineTreasuryDataFetcher.FetchDataAsync(stoppingToken, mineBuybackFullResync));
    }
}