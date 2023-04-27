using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite.Dapper;

namespace SapientFi.Infrastructure.Migrations;

public class M_202304270909_index_id: MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();
        
        /* *********************************************************************************
         */
        builder.Step("create index on id in terra raw", () =>
        {
            connection.Execute(@"
create index terra2_raw_transaction_entity_id_index
    on public.terra2_raw_transaction_entity (id);
");
        });
        
        // TODO Why is amount using 38 digits numeric precision? It seems rather excessive, none of the
        // built-in types are even that big. Ain't uluna represented as a uint64? That's only 20 digits.
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
        
        /* *********************************************************************************
         */
        builder.Step("create index on id on kujira raw", () =>
        {
            connection.Execute(@"
create index kujira_raw_transaction_entity_id_index
    on public.kujira_raw_transaction_entity (id);
");
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}