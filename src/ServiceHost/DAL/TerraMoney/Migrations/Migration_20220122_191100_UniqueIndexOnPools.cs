using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220122_191100_UniqueIndexOnPools : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("clean up duped row in pools", () =>
        {
            connection.Execute(@"
delete from terra_pylon_pool_entity where id = 933726875711176704;
");
        });
        builder.Step("create unique depositor pr pool and transaction index", () =>
        {
            connection.Execute(@"
create unique index terra_pylon_pool_entity_dep_tx_fn_creat_uindex
    on terra_pylon_pool_entity (depositor, transaction_id, friendly_name, created_at);
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}