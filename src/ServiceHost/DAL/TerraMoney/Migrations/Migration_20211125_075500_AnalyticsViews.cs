using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Invacoil.Data.Migrations
{
    public class Migration_20211125_075500_AnalyticsViews : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();

            builder.Step("create first stake view", () =>
            {
                connection.Execute(@"
create or replace view v_wallet_first_stake(sender, min, days_to_now) as
	SELECT terra_mine_staking_entity.sender,
       min(terra_mine_staking_entity.created_at)                                 AS min,
       date_part('day'::text, now() - min(terra_mine_staking_entity.created_at)) AS days_to_now
FROM terra_mine_staking_entity
GROUP BY terra_mine_staking_entity.sender
ORDER BY (min(terra_mine_staking_entity.created_at));
");
            });
            builder.Step("create stake exit view", () =>
            {
                connection.Execute(@"
create view v_wallet_stake_exit(sender, max, sum) as
	SELECT terra_mine_staking_entity.sender,
       max(terra_mine_staking_entity.created_at) AS max,
       sum(terra_mine_staking_entity.amount)     AS sum
FROM terra_mine_staking_entity
GROUP BY terra_mine_staking_entity.sender
HAVING sum(terra_mine_staking_entity.amount) < 1::numeric
ORDER BY (max(terra_mine_staking_entity.created_at));
");
            });
            
            builder.Step("create stake sum", () =>
            {
                connection.Execute(@"
create view v_wallet_stake_sum(sender, sum) as
	SELECT terra_mine_staking_entity.sender,
       sum(terra_mine_staking_entity.amount) AS sum
FROM terra_mine_staking_entity
GROUP BY terra_mine_staking_entity.sender
HAVING sum(terra_mine_staking_entity.amount) >= 1::numeric
ORDER BY (sum(terra_mine_staking_entity.amount)) DESC;
");
            });
            
            builder.Step("create stake sum v2", () =>
            {
                connection.Execute(@"
create view v_wallet_stake_sum_v2(wallet, sum, staked_since) as
	SELECT terra_mine_staking_entity.sender          AS wallet,
       sum(terra_mine_staking_entity.amount)     AS sum,
       min(terra_mine_staking_entity.created_at) AS staked_since
FROM terra_mine_staking_entity
GROUP BY terra_mine_staking_entity.sender
ORDER BY (sum(terra_mine_staking_entity.amount)) DESC;
");
            });
            
            builder.Step("create stake percentiles view", () =>
            {
                connection.Execute(@"
create view v_wallet_stake_percentiles(p_99, p_95, p_90, p_75, median, average, max, min) as
	SELECT percentile_disc(0.99::double precision) WITHIN GROUP (ORDER BY v_wallet_stake_sum.sum) AS p_99,
       percentile_disc(0.95::double precision) WITHIN GROUP (ORDER BY v_wallet_stake_sum.sum) AS p_95,
       percentile_disc(0.90::double precision) WITHIN GROUP (ORDER BY v_wallet_stake_sum.sum) AS p_90,
       percentile_disc(0.75::double precision) WITHIN GROUP (ORDER BY v_wallet_stake_sum.sum) AS p_75,
       percentile_disc(0.50::double precision) WITHIN GROUP (ORDER BY v_wallet_stake_sum.sum) AS median,
       avg(v_wallet_stake_sum.sum)                                                            AS average,
       max(v_wallet_stake_sum.sum)                                                            AS max,
       min(v_wallet_stake_sum.sum)                                                            AS min
FROM v_wallet_stake_sum;
");
            });
            
            builder.Step("create gw pool white whale view", () =>
            {
                connection.Execute(@"
CREATE OR REPLACE VIEW v_gw_pool_white_whale AS
select * from terra_pylon_pool_entity
where friendly_name in ('WhiteWhale3','WhiteWhale2','WhiteWhale1');
");
            });
            
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}