using System;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Invacoil.Data.Migrations
{
    public class Migration_20211114_192600_AddIsBuyBack : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();

            builder.Step("truncate mine staking table", () =>
            {
                connection.Execute(@"
truncate table terra_mine_staking_entity;
");
            });
            
            builder.Step("add is buy back to mine staking table", () =>
            {
                connection.Execute(@"
alter table terra_mine_staking_entity
	add is_buy_back boolean not null default false;

create index idx_terraminestakingentity_isbuyback
	on terra_mine_staking_entity (is_buy_back);
");
            });
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}