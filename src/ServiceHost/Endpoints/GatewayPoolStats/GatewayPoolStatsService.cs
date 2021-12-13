using System.Text;
using NewRelic.Api.Agent;
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
    
    [Trace]
    public async Task<GatewayPoolStatsGraph> GetItAsync(
        GatewayPoolIdentifier gatewayIdentifier,
        CancellationToken cancellationToken
    )
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var friendlyNames = PoolIdentifierToFriendlyNames(gatewayIdentifier);

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

    [Trace]
    private static TerraPylonPoolFriendlyName[] PoolIdentifierToFriendlyNames(
        GatewayPoolIdentifier gatewayPoolIdentifier
    )
    {
        return gatewayPoolIdentifier switch
        {
            GatewayPoolIdentifier.WhiteWhale => new[]
            {
                TerraPylonPoolFriendlyName.WhiteWhale1,
                TerraPylonPoolFriendlyName.WhiteWhale2,
                TerraPylonPoolFriendlyName.WhiteWhale3
            },
            GatewayPoolIdentifier.Loop => new []
            {
                TerraPylonPoolFriendlyName.Loop1,
                TerraPylonPoolFriendlyName.Loop2,
                TerraPylonPoolFriendlyName.Loop3,
            },
            GatewayPoolIdentifier.Orion => new []
            {
                TerraPylonPoolFriendlyName.Orion,
            },
            GatewayPoolIdentifier.Valkyrie => new []
            {
                TerraPylonPoolFriendlyName.Valkyrie1,
                TerraPylonPoolFriendlyName.Valkyrie2,
                TerraPylonPoolFriendlyName.Valkyrie3,
            },
            GatewayPoolIdentifier.TerraWorld => new []
            {
                TerraPylonPoolFriendlyName.TerraWorld1,
                TerraPylonPoolFriendlyName.TerraWorld2,
                TerraPylonPoolFriendlyName.TerraWorld3,
            },
            GatewayPoolIdentifier.Mine => new []
            {
                TerraPylonPoolFriendlyName.Mine1,
                TerraPylonPoolFriendlyName.Mine2,
                TerraPylonPoolFriendlyName.Mine3,
            },
            GatewayPoolIdentifier.Nexus => new []
            {
                TerraPylonPoolFriendlyName.Nexus,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(gatewayPoolIdentifier), gatewayPoolIdentifier, null)
        };
    }
}