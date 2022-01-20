using System.Data;
using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.DAL.TerraMoney.Views;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.RecurringJobs;

public class MaterializedViewRefresherJob
{
    private readonly ILogger<MaterializedViewRefresherJob> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MaterializedViewRefresherJob(
        ILogger<MaterializedViewRefresherJob> logger,
        IEnabledServiceRolesConfig serviceRolesConfig,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _logger = logger;
        _serviceRolesConfig = serviceRolesConfig;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Background worker role not active, not starting materialized view refrsher");
            return;
        }
        
        _logger.LogInformation("Materialized view refresher job triggered - refreshing views");

        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: stoppingToken);
        {
            db.SetCommandTimeout((int?)TimeSpan.FromMinutes(15).TotalSeconds);

            await RefreshViewAsync<GatewayPoolDepositorRankingView>(stoppingToken, db);
            await RefreshViewAsync<MineWalletStakeView>(stoppingToken, db);
            await RefreshViewAsync<MineWalletStakePercentilesView>(stoppingToken, db);
        }
    }

    private async Task RefreshViewAsync<T>(CancellationToken stoppingToken, IDbConnection db)
    {
        var viewName = db.GetDialectProvider().GetTableName(ModelDefinition<T>.Definition);
        await db.ExecuteNonQueryAsync($"REFRESH MATERIALIZED VIEW {viewName};", token: stoppingToken);
        _logger.LogInformation("Done refreshing {ViewName}", viewName);
    }
}