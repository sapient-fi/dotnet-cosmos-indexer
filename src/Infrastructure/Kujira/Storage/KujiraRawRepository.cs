using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace SapientFi.Infrastructure.Kujira.Storage;

public class KujiraRawRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public KujiraRawRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public virtual async Task SaveRawTransactionAsync(KujiraRawTransactionEntity entity, CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAsync(entity, token: cancellationToken);
    }

    public virtual async Task<int> GetLatestSeenBlockHeightAsync(CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);

        return await db.ScalarAsync<KujiraRawTransactionEntity, int>(x => Sql.Max(x.Height), _ => true, cancellationToken);
    }

    public virtual async Task<KujiraRawTransactionEntity?> GetByIdOrDefaultAsync(long id, CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        return await db.SingleByIdAsync<KujiraRawTransactionEntity>(id, cancellationToken);
    }
}