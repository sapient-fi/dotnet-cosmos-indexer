using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220213_185900_TerraLiquidityPool : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("add terra liquidity pool table", () =>
        {
            connection.Execute(@"
create table terra_liquidity_pool_entity
(
    id             bigint                   not null,
    created_at     timestamp with time zone not null,
    transaction_id bigint                   not null,
    contract_addr  text,
    sender_addr    text,
    offer_asset    text,
    offer_amount   numeric(38, 6)           not null,
    ask_asset      text,
    ask_amount     numeric(38, 6)           not null,
    constraint terra_liquidity_pool_entity_pkey
			primary key (id, created_at)
);

create index idx_terraliquiditypoolentity_transactionid
    on terra_liquidity_pool_entity (transaction_id);

create index idx_terraliquiditypoolentity_contractaddr
    on terra_liquidity_pool_entity (contract_addr);

create index idx_terraliquiditypoolentity_senderaddr
    on terra_liquidity_pool_entity (sender_addr);

SELECT create_hypertable('terra_liquidity_pool_entity', 'created_at', chunk_time_interval => INTERVAL '14 days');
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}