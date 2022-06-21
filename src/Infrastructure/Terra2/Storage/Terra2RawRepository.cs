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

    public virtual async Task SaveRawTransactionAsync(Terra2RawTransactionEntity entity, CancellationToken cancellationToken)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAsync(entity, token: cancellationToken);
    }

    public virtual async Task<int> GetLatestSeenBlockHeightAsync(CancellationToken cancellationToken)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);

        return await db.ScalarAsync<Terra2RawTransactionEntity, int>(x => Sql.Max(x.Height), _ => true, cancellationToken);
    }
}