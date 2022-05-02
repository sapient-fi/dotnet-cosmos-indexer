using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220226_122300_RenameLPTable : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("rename terra_liquidity_pool_entity", () =>
        {
            connection.Execute(@"
ALTER TABLE terra_liquidity_pool_entity  RENAME TO terra_liquidity_pool_trades_entity;
");
        });
        
        builder.Step("rename terra_lp_farm_entity", () =>
        {
            connection.Execute(@"
alter table terra_lp_farm_entity    rename to terra_liquidity_pool_pair_entity;
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}