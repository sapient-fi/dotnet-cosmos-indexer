using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211019_201600_TerraMoneyMine : MigrationBase
{
	protected override void ConfigureUpgrade(IMigrationBuilder builder)
	{
		var ctx = ContextAs<PostgreSqlMigrationContext>();
		var connection = ctx.ConnectionProvider.Default();
		builder.Step("enable timescaledb", () =>
		{
			connection.Execute("CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;");
		});
            
		builder.Step("create mine staking table", () =>
		{
			connection.Execute(@"
create table terra_mine_staking_entity
(
	sender text,
	amount numeric(38,6) not null,
	id bigint not null,
	created_at timestamp with time zone not null,
	tx_hash text,
	constraint terra_mine_staking_entity_pkey
			primary key (id, created_at)
);

SELECT create_hypertable('terra_mine_staking_entity', 'created_at', chunk_time_interval => INTERVAL '1 day');
");
		});
		builder.Step("create raw transaction table", () =>
		{
			connection.Execute(@"
create table terra_raw_transaction_entity
(
	id bigint not null,
	created_at timestamp with time zone not null,
	raw_tx jsonb,
	tx_hash text,
	constraint terra_raw_transaction_pkey
		primary key (id, created_at)
);
SELECT create_hypertable('terra_raw_transaction_entity', 'created_at', chunk_time_interval => INTERVAL '1 day');
");
		});
	}

	protected override void ConfigureDowngrade(IMigrationBuilder builder)
	{
		throw new System.NotImplementedException();
	}
}