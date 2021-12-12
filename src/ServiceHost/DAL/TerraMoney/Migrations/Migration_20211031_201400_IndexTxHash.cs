using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211031_201400_IndexTxHash : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();
        builder.Step("index txhas on staking entity table", () =>
        {
            connection.Execute(@"
create index terra_mine_staking_entity_tx_hash_index
	on terra_mine_staking_entity (tx_hash);
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}