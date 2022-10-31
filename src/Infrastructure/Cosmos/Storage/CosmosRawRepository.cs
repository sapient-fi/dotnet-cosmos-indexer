using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace SapientFi.Infrastructure.Cosmos.Storage;

public abstract class CosmosRawRepository<TRawTransactionEntity>
    where TRawTransactionEntity : ICosmosRawTransactionEntity
{
    protected readonly IDbConnectionFactory DbFactory;

    protected CosmosRawRepository(IDbConnectionFactory dbFactory)
    {
        DbFactory = dbFactory;
    }
    
    public async Task SaveRawTransactionAsync(
        TRawTransactionEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAsync(entity, token: cancellationToken);
    }

    public async Task<int> GetLatestSeenBlockHeightAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(cancellationToken);
        return await db.ScalarAsync<TRawTransactionEntity, int>(
            x => Sql.Max(x.Height),
            _ => true,
            cancellationToken
        );
    }

    public async Task<TRawTransactionEntity?> GetByIdOrDefaultAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(cancellationToken);
        return await db.SingleByIdAsync<TRawTransactionEntity>(id, cancellationToken);
    }
}
