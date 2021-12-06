using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Invacoil.Data.Migrations
{
    public class Migration_20211104_200500_AddLpFarms : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();
            
            builder.Step("add lp farm table", () =>
            {
                connection.Execute(@"
create table terra_lp_farm_entity
(
	id bigint not null
		constraint terra_lp_farm_entity_pkey
			primary key,
	transaction_id bigint not null,
                    asset_one_denominator text,
                asset_one_quantity numeric(38,6) not null,
                asset_one_ust_value numeric(38,6),
                asset_two_denominator text,
                    asset_two_quantity numeric(38,6) not null,
                asset_two_ust_value numeric(38,6),
                asset_lp_quantity numeric(38,6) not null,
                farm text,
                    created_at timestamp with time zone not null
                    );        
");
            });

            builder.Step("index farm column", () =>
            {
                connection.Execute(@"
create index terra_lp_farm_entity_farm_index
	on terra_lp_farm_entity (farm);
");
            });
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}