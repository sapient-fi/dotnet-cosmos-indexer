using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite.Dapper;

namespace SapientFi.Infrastructure.Migrations;

public class M_20220624_1049_from_scratch: MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();
        
        /* *********************************************************************************
         */
        builder.Step("enable timescaledb", () =>
        {
            connection.Execute("CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;");
        });
	    
        
        /* *********************************************************************************
         */
        builder.Step("create raw terra2 transaction table", () =>
        {
            connection.Execute(@"
create table terra2_raw_transaction_entity
(
    id         bigint                   not null,
    raw_tx     jsonb,
    created_at timestamp with time zone not null,
    tx_hash    text,
    height     integer                  not null,
    
    constraint terra2_raw_transaction_entity_pkey
			primary key (id, created_at),
			
	constraint terra2_raw_transaction_entity_uniqueness
			unique (tx_hash, created_at)
);

create index idx_terra2rawtransactionentity_height
    on terra2_raw_transaction_entity (height);

SELECT create_hypertable('terra2_raw_transaction_entity', 'created_at', chunk_time_interval => INTERVAL '1 day');
");
        });
        
        // TODO Why is amount using 38 digits numeric precision? It seems rather excessive, none of the
        // built-in types are even that big. Ain't uluna represented as a uint64? That's only 20 digits.
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
        
        /* *********************************************************************************
         */
        builder.Step("create terra2 delegations table", () =>
        {
            connection.Execute(@"
create table terra2_validator_delegation_ledger_entity
(
    id         bigint                   not null,
    at timestamp with time zone not null,
    tx_hash    text,
    validator_address text,
    delegator_address text,
    amount numeric(38, 6) not null,
    denominator varchar,
    
    constraint terra2_validator_delegation_ledger_entity_pkey
			primary key (id, at)
);

create index idx_terra2validatordelegationledgerentity_txhash
    on terra2_validator_delegation_ledger_entity (tx_hash);

create index idx_terra2validatordelegationledgerentity_valaddr
        on terra2_validator_delegation_ledger_entity (validator_address);

create index idx_terra2validatordelegationledgerentity_deladdr
    on terra2_validator_delegation_ledger_entity (delegator_address);

SELECT create_hypertable('terra2_validator_delegation_ledger_entity', 'at', chunk_time_interval => INTERVAL '1 day');
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}