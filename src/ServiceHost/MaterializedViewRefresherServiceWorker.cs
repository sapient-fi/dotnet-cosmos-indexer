using Pylonboard.Kernel;
using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.DAL.TerraMoney.Views;
using Pylonboard.ServiceHost.Oracles.ArbNotifier;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pylonboard.ServiceHost;

public class MaterializedViewRefresherServiceWorker : IScopedBackgroundServiceWorker
{
    private readonly ILogger<MaterializedViewRefresherServiceWorker> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MaterializedViewRefresherServiceWorker(
        ILogger<MaterializedViewRefresherServiceWorker> logger,
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
            _logger.LogInformation("Background worker role not active, not starting arb bot");
            return;
        }
        
        do
        {
            _logger.LogInformation("Materialized view refresher is entering hibernation");
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            _logger.LogInformation("Materialized view refresher back from hibernation - refreshing views");
            
            using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: stoppingToken);
            {   
                db.SetCommandTimeout((int?)TimeSpan.FromMinutes(15).TotalSeconds);
                var viewName = db.GetDialectProvider().GetTableName(ModelDefinition<GatewayPoolDepositorRankingView>.Definition);
                await db.ExecuteNonQueryAsync($"REFRESH MATERIALIZED VIEW {viewName};",
                    token: stoppingToken);
                _logger.LogInformation("Done refreshing {ViewName}", viewName);
            }
        } while (!stoppingToken.IsCancellationRequested);
    }
}