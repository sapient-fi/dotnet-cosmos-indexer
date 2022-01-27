using System;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220126_132900_IndexTxIdOnStaking : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("clean up duplicate transactions on mine staking (that are not buybacks)", () =>
        {
            var ids = connection.SqlList<long>(@"SELECT transaction_id
            FROM (SELECT *,
                count(*)
            OVER
                (PARTITION BY
            transaction_id,
            created_at
                ) AS count
            FROM terra_mine_staking_entity where is_buy_back = false) tableWithCount
                WHERE tableWithCount.count > 1;");

            connection.Delete<TerraMineStakingEntity>(q => ids.Contains(q.Id));
        });
        
        builder.Step("create idx on (tx_id, created_at) on mine staking", () =>
        {
            connection.Execute(@"
create index terra_mine_staking_entity_transaction_id_created_at_uindex
    on terra_mine_staking_entity (transaction_id, created_at);
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}