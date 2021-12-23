using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211223_105100_GwPoolMateralizedViewRedo : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("create mine stake sums materialized view", () =>
        {
            connection.Execute(@"
CREATE MATERIALIZED VIEW mv_wallet_stake_sum(wallet, sum, staked_since) as
SELECT terra_mine_staking_entity.sender          AS wallet,
       sum(terra_mine_staking_entity.amount)     AS sum,
       min(terra_mine_staking_entity.created_at) AS staked_since
FROM terra_mine_staking_entity
GROUP BY terra_mine_staking_entity.sender
ORDER BY (sum(terra_mine_staking_entity.amount)) DESC;

CREATE INDEX mv_wallet_stake_sum_index
  ON mv_wallet_stake_sum (wallet);
");
        });

        builder.Step("drop gateway deposit materialized view", () =>
        {
            connection.Execute(@"drop materialized view mv_gateway_pool_staker_ranking;");
        });
        
        builder.Step("recreate gateway deposit materialized view", () =>
        {
            connection.Execute(@"
CREATE MATERIALIZED VIEW mv_gateway_pool_staker_ranking AS
SELECT PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY terra_pylon_pool_entity.amount) AS deposit_amount_median,
       Avg(terra_pylon_pool_entity.amount)                                         AS deposit_amount_avg,
       Sum(terra_pylon_pool_entity.amount)                                         AS deposit_amount_sum,
       Min(terra_pylon_pool_entity.amount)                                         AS deposit_amount_min,
       Max(terra_pylon_pool_entity.amount)                                         AS deposit_amount_max,
       SUM(v_wallet_stake_sum_v2.sum)                                              AS staking_amount,
       terra_pylon_pool_entity.friendly_name
FROM terra_pylon_pool_entity
         LEFT JOIN v_wallet_stake_sum_v2
                   ON (terra_pylon_pool_entity.depositor = v_wallet_stake_sum_v2.wallet)
WHERE terra_pylon_pool_entity.operation = 'Deposit'
GROUP BY terra_pylon_pool_entity.depositor, friendly_name
ORDER BY Sum(terra_pylon_pool_entity.amount) DESC;

CREATE INDEX mv_gateway_pool_staker_ranking_index
  ON mv_gateway_pool_staker_ranking (friendly_name, staking_amount);
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}