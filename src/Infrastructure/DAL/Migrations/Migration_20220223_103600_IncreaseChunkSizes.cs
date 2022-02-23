using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220223_103600_IncreaseChunkSizes : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("increase mine staking chunk size", () =>
        {
            connection.Execute(@"
SELECT set_chunk_time_interval('terra_mine_staking_entity', INTERVAL '14 days');
");
        });
        builder.Step("increase raw tx chunk size", () =>
        {
            connection.Execute(@"
SELECT set_chunk_time_interval('terra_raw_transaction_entity', INTERVAL '7 days');
");
        });
        builder.Step("increase pylon pool chunk size", () =>
        {
            connection.Execute(@"
SELECT set_chunk_time_interval('terra_pylon_pool_entity', INTERVAL '30 days');
");
        });
        builder.Step("increase liquidity pool chunk size", () =>
        {
            connection.Execute(@"
SELECT set_chunk_time_interval('terra_liquidity_pool_entity', INTERVAL '90 days');
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}