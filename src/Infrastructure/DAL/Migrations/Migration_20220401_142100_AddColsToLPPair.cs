using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220401_142100_AddColsToLPPair : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("add cols to terra_lp_pair table", () =>
        {
            connection.Execute(@"
alter table terra_liquidity_pool_pair_entity
    add dex text default 'TerraSwap' not null;

alter table terra_liquidity_pool_pair_entity
    add wallet text not null default '';

create index terra_liquidity_pool_pair_entity_dex_index
    on terra_liquidity_pool_pair_entity (dex);
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}