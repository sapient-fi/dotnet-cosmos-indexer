using Npgsql;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace SapientFi.Infrastructure.Terra2.Storage;

public class Terra2RawRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public Terra2RawRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public virtual async Task SaveRawTransactionAsync(Terra2RawTransactionEntity entity, CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAsync(entity, token: cancellationToken);
    }

    public virtual async Task<int> GetLatestSeenBlockHeightAsync(CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);

        return await db.ScalarAsync<Terra2RawTransactionEntity, int>(x => Sql.Max(x.Height), _ => true, cancellationToken);
    }

    public virtual async Task<Terra2RawTransactionEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        return await db.SingleByIdAsync<Terra2RawTransactionEntity>(id, cancellationToken);
    }
}