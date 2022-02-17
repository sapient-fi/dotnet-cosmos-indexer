using System;
using Pylonboard.Infrastructure.Hosting.TerraDataFetchers;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using TerraDotnet;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220203_20060_FinalMineStakBlock : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("get the final (very first) transactions on mine staking contract", async () =>
        {
            var fetcher = ctx.Container.Resolve<MineStakingDataFetcher>();
            var txEnumerator = ctx.Container.Resolve<TerraTransactionEnumerator>();

            await foreach (var (tx, msg) in txEnumerator.EnumerateTransactionsAsync(
                               121079826,
                               100,
                               TerraStakingContracts.MINE_STAKING_CONTRACT,
                               CancellationToken.None))
            {
                await fetcher.ProcessSingleTransactionAsync(tx, msg, null, connection, true, CancellationToken.None);
            }
        });
        
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}