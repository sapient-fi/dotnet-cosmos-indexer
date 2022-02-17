using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211221_203000_GwPoolMateralizedView : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("index sender on mine staking", () =>
        {
            connection.Execute(@"
create index terra_mine_staking_entity_sender_index
    on terra_mine_staking_entity (sender);
");
        });

        builder.Step("index depositor on gateway pools", () =>
        {
            connection.Execute(@"
create index terra_pylon_pool_entity_depositor_index
    on terra_pylon_pool_entity (depositor);
");
        });
        
        builder.Step("create gateway deposit materialized view", () =>
        {
            connection.Execute(@"
CREATE MATERIALIZED VIEW mv_gateway_pool_staker_ranking AS
SELECT PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY terra_pylon_pool_entity.amount) AS deposit_amount_median,
       Avg(terra_pylon_pool_entity.amount)                                     AS deposit_amount_avg,
        Sum(terra_pylon_pool_entity.amount)                                     AS deposit_amount_sum,
        Min(terra_pylon_pool_entity.amount)                                     AS deposit_amount_min,
        Max(terra_pylon_pool_entity.amount)                                     AS deposit_amount_max,
    SUM(terra_mine_staking_entity.amount)                                       AS staking_amount,
                terra_pylon_pool_entity.friendly_name
                FROM terra_pylon_pool_entity
            LEFT JOIN terra_mine_staking_entity
            ON (terra_pylon_pool_entity.depositor = terra_mine_staking_entity.sender)
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