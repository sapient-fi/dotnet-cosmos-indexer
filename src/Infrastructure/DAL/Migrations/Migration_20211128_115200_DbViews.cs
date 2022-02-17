using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211128_115200_DbViews : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("create gw pool loop view", () =>
        {
            connection.Execute(@"
CREATE OR REPLACE VIEW v_gw_pool_loop AS
select *
from terra_pylon_pool_entity
where friendly_name in ('Loop1', 'Loop2', 'Loop3');
");
        });
        builder.Step("create gw pool loop vs mine staked", () =>
        {
            connection.Execute(@"
create or replace view v_gw_pool_loop_vs_mine_staked(pool_deposit_amount, depositor, mine_stake_amount, is_staking_mine) as
	SELECT sum(gw_pool.amount) AS pool_deposit_amount,
       gw_pool.depositor,
       sum(stake.amount)      AS mine_stake_amount,
       CASE
           WHEN sum(stake.amount) IS NULL THEN false
           ELSE true
           END             AS is_staking_mine
FROM v_gw_pool_loop gw_pool
         LEFT JOIN terra_mine_staking_entity stake ON gw_pool.depositor = stake.sender
GROUP BY gw_pool.depositor
ORDER BY (sum(gw_pool.amount)) DESC;

");
        });
            
        builder.Step("create gw pool white whale vs mine staked", () =>
        {
            connection.Execute(@"
create or replace view v_gw_pool_white_whale_vs_mine_staked(pool_deposit_amount, depositor, mine_stake_amount, is_staking_mine) as
	SELECT sum(gw_pool.amount) AS pool_deposit_amount,
       gw_pool.depositor,
       sum(stake.amount)      AS mine_stake_amount,
       CASE
           WHEN sum(stake.amount) IS NULL THEN false
           ELSE true
           END             AS is_staking_mine
FROM v_gw_pool_white_whale gw_pool
         LEFT JOIN terra_mine_staking_entity stake ON gw_pool.depositor = stake.sender
GROUP BY gw_pool.depositor
ORDER BY (sum(gw_pool.amount)) DESC;

");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}