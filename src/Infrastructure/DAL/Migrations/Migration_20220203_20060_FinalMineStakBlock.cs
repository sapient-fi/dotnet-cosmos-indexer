using System;
using System.Text.Json;
using MassTransit;
using Pylonboard.Infrastructure.Hosting.TerraDataFetchers;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra;
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
            var bus = ctx.Container.Resolve<IBus>();
            var txEnumerator = ctx.Container.Resolve<TerraTransactionEnumerator>();
            var db = ctx.ConnectionProvider.Default();
            await foreach (var (tx, msg) in txEnumerator.EnumerateTransactionsAsync(
                               121079826,
                               100,
                               TerraStakingContracts.MINE_STAKING_CONTRACT,
                               CancellationToken.None))
            {
                
                await db.SaveAsync(obj: new TerraRawTransactionEntity
                    {
                        Id = tx.Id,
                        CreatedAt = tx.CreatedAt,
                        TxHash = tx.TransactionHash,
                        RawTx = JsonSerializer.Serialize(tx),
                    }
                );
                await bus.Publish(new MineStakingTransactionMessage
                {
                    TransactionId = tx.Id
                });
            }
        });
        
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}