using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20211125_074800_AddGwPools : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("create gw pool table", () =>
        {
            connection.Execute(@"
create table terra_pylon_pool_entity
(
	id bigint not null,
	transaction_id bigint not null,
    depositor text,
    pool_contract text,
    amount numeric(38,6) not null,
    denominator text,
    operation varchar(255) not null,
    friendly_name varchar(255) not null,
    created_at timestamp with time zone not null,
    constraint terra_pylon_pool_entity_pkey
			primary key (id, created_at)
                    );

SELECT create_hypertable('terra_pylon_pool_entity', 'created_at', chunk_time_interval => INTERVAL '1 day');
");
                
        });

        builder.Step("add index to friendly name", () =>
        {
            connection.Execute(@"
create index terra_pylon_pool_entity_friendly_name_idx
	on terra_pylon_pool_entity (friendly_name);

");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}