using System.Collections;
using System.Data;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;

public class Terra2DelegationsRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public Terra2DelegationsRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public virtual async Task SaveAsync(Terra2ValidatorDelegationLedgerEntity entity, CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAsync(entity, token: cancellationToken);
    }
    
    public virtual async Task SaveAllAsync(IEnumerable<Terra2ValidatorDelegationLedgerEntity> entities, CancellationToken cancellationToken = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAllAsync(entities, token: cancellationToken);
    }

    public virtual async Task<T1> SingleAsync<T1, T2>(SqlExpression<T2> sql, CancellationToken ct = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(ct);
        return await db.SingleAsync<T1>(sql, ct);
    }

    public virtual async Task<IEnumerable<T1>> SelectAsync<T1, T2>(SqlExpression<T2> sql, CancellationToken ct = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(ct);
        return await db.SelectAsync<T1>(sql, ct);
    }
    
    public virtual async Task<T1> SingleByIdAsync<T1, T2>(T2 id, CancellationToken ct = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(ct);
        return await db.SingleByIdAsync<T1>(id, ct);
    }
    
    public virtual async Task<List<T>> SelectByIds<T>(IEnumerable ids, CancellationToken ct = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(ct);
        return await db.SelectByIdsAsync<T>(ids, ct);
    }

    public virtual async Task<T1> ScalarAsync<T1, T2>(SqlExpression<T2> sql, CancellationToken ct = default)
    {
        using var db = await _dbFactory.OpenDbConnectionAsync(ct);
        return await db.ScalarAsync<T1>(sql, ct);
    }

    public virtual async Task<IDbConnection> GetDbConnectionAsync(CancellationToken cancellationToken = new())
    {
        return await _dbFactory.OpenDbConnectionAsync(cancellationToken);
    }
    
    /*/
    public virtual async Task<IEnumerable<T>> QueryAsync<T>(string query, CancellationToken cancellationToken)
    {
        var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);
        return await db.QueryAsync<T>(query);
    }

    public virtual async Task<T1> SingleAsync<T1, T2>(SqlExpression<T2> sqlExpression, CancellationToken cancellationToken = new())
    {
        var db = await _dbFactory.OpenDbConnectionAsync(cancellationToken);

        return await db.SingleAsync<T1>(sqlExpression, cancellationToken);
    }
    ////*/
}