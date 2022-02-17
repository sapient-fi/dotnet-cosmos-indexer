using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220121_195400_MyGwPoolsView : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("create my gateway pools materialized view", () =>
        {
            connection.Execute(@"
CREATE MATERIALIZED VIEW mv_my_gateway_pools AS
 select SUM(amount) as amount,
       denominator,
       operation,
       friendly_name,
       pool_contract,
       depositor
from terra_pylon_pool_entity
group by friendly_name, operation, denominator, pool_contract, depositor
order by friendly_name;
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}