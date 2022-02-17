using Hangfire;
using Hangfire.Common;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.RecurringJobs;

namespace Pylonboard.ServiceHost;

public class CronjobManager
{
    private readonly IEnabledServiceRolesConfig _rolesConfig;
    private readonly ILogger<CronjobManager> _logger;
    private readonly RecurringJobManager _jobManager;
    private readonly IFeatureConfig _featureConfig;

    public CronjobManager(
        IEnabledServiceRolesConfig rolesConfig,
        ILogger<CronjobManager> logger,
        RecurringJobManager jobManager,
        IFeatureConfig featureConfig
    )
    {
        _rolesConfig = rolesConfig;
        _logger = logger;
        _jobManager = jobManager;
        _featureConfig = featureConfig;
    }

    public virtual Task RegisterJobsIfRequiredAsync()
    {
        if (!_rolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Background worker role not enabled, not registering jobs");
        }

        _jobManager.AddOrUpdate("psi-arbs",
            Job.FromExpression<PsiPoolArbJob>((job => job.DoWorkAsync(CancellationToken.None))),
            "*/5 * * * *"
        );

        _jobManager.AddOrUpdate("terra-money-1",
            Job.FromExpression<TerraMoneyRefreshJob>(
                job => job.DoWorkAsync(CancellationToken.None, false, false, false)),
            "33 * * * *"
        );

        _jobManager.AddOrUpdate("terra-money-2",
            Job.FromExpression<TerraMoneyRefreshJob>(
                job => job.DoWorkAsync(CancellationToken.None, false, false, false)),
            "03 * * * *"
        );

        _jobManager.AddOrUpdate("materialized-view-refresh",
            Job.FromExpression<MaterializedViewRefresherJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "13 * * * *"
        );

        _jobManager.AddOrUpdate("fx-rate-download",
            Job.FromExpression<FxRateDownloadJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "43 * * * *"
        );

        _jobManager.AddOrUpdate(
            "terra-money-bpsi-lp",
            Job.FromExpression<TerraBpsiDpLiquidityPoolRefreshJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "53 * * * *"
        );
        
        
        if (_featureConfig.TriggerGatewayPoolFullResync
            || _featureConfig.TriggerMineStakingFullResync
            || _featureConfig.TriggerMineBuyBackFullResync
           )
        {
            _logger.LogWarning("Triggering full terra money resync (config settings)");
            BackgroundJob.Enqueue<TerraMoneyRefreshJob>(job => job.DoWorkAsync(
                    CancellationToken.None,
                    _featureConfig.TriggerGatewayPoolFullResync,
                    _featureConfig.TriggerMineStakingFullResync,
                    _featureConfig.TriggerMineBuyBackFullResync
                )
            );
        }

        return Task.CompletedTask;
    }
}