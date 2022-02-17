using Microsoft.Extensions.Diagnostics.HealthChecks;
using NewRelic.Api.Agent;
using Pylonboard.Kernel.DAL.Entities.Terra;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost;

public class DbConnectionHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _factory;
    private readonly ILogger<DbConnectionHealthCheck> _logger;

    public DbConnectionHealthCheck(
        IDbConnectionFactory factory,
        ILogger<DbConnectionHealthCheck> logger)
    {
        _factory = factory;
        _logger = logger;
    }
    
    [Trace]
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            // connect and query. If it goes well, it's healthy
            using var db = await _factory.OpenDbConnectionAsync(token: cancellationToken);
            await db.SingleAsync(db.From<TerraMineStakingEntity>().Take(1), token: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "failure during db health check");
            return HealthCheckResult.Unhealthy();
        }
    }
}