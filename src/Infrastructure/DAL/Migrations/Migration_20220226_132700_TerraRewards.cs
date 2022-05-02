using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite.Dapper;

namespace Invacoil.Data.Migrations
{
    public class Migration_20211021_131700_TerraAirdrops : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();
            builder.Step("create terra reward table", () =>
            {
                connection.Execute(@"
create table terra_reward_entity
(
	id bigint not null
		constraint terra_airdrop_entity_pkey
			primary key,
	wallet text,
	from_contract text,
	amount numeric(38,6) not null,
	amount_ust_at_claim numeric(38,6),
	amount_ust_now numeric(38,6),
	denominator text,
	reward_type text not null,
	transaction_id bigint not null,
	created_at timestamp with time zone not null,
	updated_at timestamp with time zone not null
);
");
            });
            
            builder.Step("add indexes to reward table", () =>
            {
	            connection.Execute(@"
create index terra_reward_entity_reward_type_index
	on terra_reward_entity (reward_type);

create index terra_reward_entity_wallet_index
	on terra_reward_entity (wallet);

create index terra_reward_entity_contract_index
	on terra_reward_entity (from_contract);
");
            });
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}