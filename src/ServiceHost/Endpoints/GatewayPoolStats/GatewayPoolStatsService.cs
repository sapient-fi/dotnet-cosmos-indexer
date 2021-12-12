using Pylonboard.ServiceHost.DAL.TerraMoney;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;

public class GatewayPoolStatsService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GatewayPoolStatsService(
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    public async Task<GatewayPoolStatsGraph> GetItAsync(GatewayPoolIdentifier gatewayIdentifier, CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync();

        var query = db.From<TerraPylonPoolEntity>()
            .Select(entity => new
                { TotalValueLocked = Sql.Sum("amount"), MinDeposit = Sql.Min("amount"), MaxDeposit = Sql.Max("amount"), AverageDeposit = Sql.Avg("amount") })
            .Where(entity => Sql.In(entity.FriendlyName, TerraPylonPoolFriendlyName.WhiteWhale1,
                TerraPylonPoolFriendlyName.WhiteWhale2, TerraPylonPoolFriendlyName.WhiteWhale3));
        var stats = await db.SingleAsync<GatewayPoolStatsOverallGraph>(query, token: cancellationToken);

        return new GatewayPoolStatsGraph
        {
            Overall = stats
        };
    }
}