using System.Text.Json;
using Pylonboard.Infrastructure.Hosting.TerraDataFetchers;
using Pylonboard.Kernel;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.IdGeneration;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using TerraDotnet;
using TerraDotnet.TerraFcd.Messages;

namespace Invacoil.Data.Migrations
{
    public class Migration_20211220_202500_MineBuybacks : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();
            builder.Step("add mine buybacks table", () =>
            {
                connection.Execute(@"
create table terra_mine_buyback_entity
(
    id               bigint                   not null
        primary key,
    transaction_id   bigint                   not null,
    transaction_hash text,
    mine_amount      numeric(38, 6)           not null,
    ust_amount       numeric(38, 6)           not null,
    created_at       timestamp with time zone not null
);
");
            });
            builder.Step("fill table with buyback info", async () =>
            {
                var transactionEnumerator = ctx.Container.Resolve<TerraTransactionEnumerator>();
                var idGenerator = ctx.Container.Resolve<IdGenerator>();
                
                await foreach (var (tx, stdTx) in transactionEnumerator.EnumerateTransactionsAsync(
                           0,
                           100,
                           TerraStakingContracts.MINE_BUYBACK_CONTRACT,
                           CancellationToken.None
                       ))
                {
                    await connection.SaveAsync(obj: new TerraRawTransactionEntity
                        {
                            Id = tx.Id,
                            CreatedAt = tx.CreatedAt,
                            TxHash = tx.TransactionHash,
                            RawTx = JsonSerializer.Serialize(tx),
                        }
                    );
                    
                    var events = tx.Logs
                        .SelectMany(l => l.Events)
                        .Where(evt =>
                            evt.Attributes.Contains(new TxLogEventAttribute() { Key = "action", Value = "sweep" }) &&
                            evt.Type.EqualsIgnoreCase("from_contract")
                        ).ToList();

                    if (!events.Any())
                    {
                        continue;
                    }

                    foreach (var evt in events)
                    {
                        var distributeMineAmountStr =
                            evt.Attributes.First(attrib => 
                                attrib.Key.EqualsIgnoreCase("distribute_amount")).Value;
                        var mineAmount = distributeMineAmountStr.ToInt64() / 1_000_000M;
                        var offerAmountUstStr =
                            evt.Attributes.First(attribute =>
                                attribute.Key.EqualsIgnoreCase("offer_amount")).Value;
                        var offerAmountUst = offerAmountUstStr.ToInt64() / 1_000_000m;
                        
                        await connection.InsertAsync(new TerraMineBuybackEntity
                        {
                            Id = idGenerator.Snowflake(),
                            CreatedAt = tx.CreatedAt,
                            MineAmount = mineAmount,
                            UstAmount = offerAmountUst,
                            TransactionHash = tx.TransactionHash,
                            TransactionId = tx.Id,
                        });
                    }
                }
            });
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}