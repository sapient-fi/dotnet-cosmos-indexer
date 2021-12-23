using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211223_210600_MineRankingMaterialView : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("create mine stake percentiles materialized view", () =>
        {
            connection.Execute(@"
create materialized view mv_wallet_stake_percentiles(p_99, p_95, p_90, p_75, median, average, max, min) as
SELECT percentile_disc(0.99::double precision) WITHIN GROUP (ORDER BY mv_wallet_stake_sum.sum) AS p_99,
       percentile_disc(0.95::double precision) WITHIN GROUP (ORDER BY mv_wallet_stake_sum.sum) AS p_95,
       percentile_disc(0.90::double precision) WITHIN GROUP (ORDER BY mv_wallet_stake_sum.sum) AS p_90,
       percentile_disc(0.75::double precision) WITHIN GROUP (ORDER BY mv_wallet_stake_sum.sum) AS p_75,
       percentile_disc(0.50::double precision) WITHIN GROUP (ORDER BY mv_wallet_stake_sum.sum) AS median,
       avg(mv_wallet_stake_sum.sum)                                                            AS average,
       max(mv_wallet_stake_sum.sum)                                                            AS max,
       min(mv_wallet_stake_sum.sum)                                                            AS min
FROM mv_wallet_stake_sum;
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}