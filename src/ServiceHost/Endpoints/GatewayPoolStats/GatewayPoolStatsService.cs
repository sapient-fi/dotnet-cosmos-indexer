using System.Text;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Endpoints.Types;
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

    public async Task<GatewayPoolStatsGraph> GetItAsync(GatewayPoolIdentifier gatewayIdentifier,
        CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        // TODO resolve from input GW pool name
        var friendlyNames = new[]
        {
            TerraPylonPoolFriendlyName.WhiteWhale1,
            TerraPylonPoolFriendlyName.WhiteWhale2,
            TerraPylonPoolFriendlyName.WhiteWhale3
        };

        var stats = await db.SingleAsync<GatewayPoolStatsOverallGraph>(db.From<TerraPylonPoolEntity>()
                .Select(entity => new
                {
                    TotalValueLocked = Sql.Sum("amount"),
                    MinDeposit = Sql.Min("amount"),
                    MaxDeposit = Sql.Max("amount"),
                    AverageDeposit = Sql.Avg("amount")
                })
                .Where(entity => Sql.In(entity.FriendlyName, friendlyNames) && Sql.In(entity.Operation,
                    new[] { TerraPylonPoolOperation.Deposit, TerraPylonPoolOperation.Withdraw })),
            token: cancellationToken);

        stats.DepositPerWallet = await db.SqlListAsync<WalletAndDepositEntry>(
            db.From<TerraPylonPoolEntity>()
                .GroupBy(x => x.Depositor)
                .Where(entity => Sql.In(entity.FriendlyName, friendlyNames) && Sql.In(entity.Operation,
                    new[] { TerraPylonPoolOperation.Deposit, TerraPylonPoolOperation.Withdraw }))
                .Select(entity => new
                {
                    Wallet = entity.Depositor, Amount = Sql.Sum("amount")
                }), token: cancellationToken);

        stats.DepositsOverTime = await db.SqlListAsync<TimeSeriesStatEntry>(
            db.From<TerraPylonPoolEntity>()
                .Select("SUM(amount) as Value, DATE(created_at) as At")
                .GroupBy("DATE(created_at)")
                .Where(entity => Sql.In(entity.FriendlyName, friendlyNames)
                                 && Sql.In(entity.Operation,
                                     new[] { TerraPylonPoolOperation.Deposit, TerraPylonPoolOperation.Withdraw }))
            , token: cancellationToken);

        return new GatewayPoolStatsGraph
        {
            Overall = stats
        };
    }

    private string ToInSqlQuery(IEnumerable<TerraPylonPoolFriendlyName> friendlyNames)
    {
        var sb = new StringBuilder();
        sb.Append("IN(");
        foreach (var item in friendlyNames)
        {
            sb.Append($"'{item.ToString()}',");
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(")");
        return sb.ToString();
    }
}