using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace SapientFi.Infrastructure.Cosmos.Indexers.Delegations.Storage;

public abstract class CosmosDelegationsRepository<TValidatorDelegationLedgerEntity>
    where TValidatorDelegationLedgerEntity : ICosmosValidatorDelegationLedgerEntity
{
    protected readonly IDbConnectionFactory DbFactory;

    protected CosmosDelegationsRepository(IDbConnectionFactory dbFactory)
    {
        DbFactory = dbFactory;
    }

    public async Task SaveAsync(
        TValidatorDelegationLedgerEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAsync(entity, token: cancellationToken);
    }

    public async Task SaveAllAsync(
        IEnumerable<TValidatorDelegationLedgerEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(cancellationToken);
        await db.SaveAllAsync(entities, token: cancellationToken);
    }

    public async Task<T1> SingleAsync<T1, T2>(SqlExpression<T2> sql, CancellationToken ct = default)
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(ct);
        return await db.SingleAsync<T1>(sql, ct);
    }

    public async Task<IEnumerable<T1>> SelectAsync<T1, T2>(
        SqlExpression<T2> sql,
        CancellationToken ct = default
    )
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(ct);
        return await db.SelectAsync<T1>(sql, ct);
    }

    public async Task<T1> SingleByIdAsync<T1, T2>(T2 id, CancellationToken ct = default)
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(ct);
        return await db.SingleByIdAsync<T1>(id, ct);
    }

    public async Task<List<T>> SelectByIds<T>(IEnumerable ids, CancellationToken ct = default)
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(ct);
        return await db.SelectByIdsAsync<T>(ids, ct);
    }

    public async Task<T1> ScalarAsync<T1, T2>(SqlExpression<T2> sql, CancellationToken ct = default)
    {
        using IDbConnection? db = await DbFactory.OpenDbConnectionAsync(ct);
        return await db.ScalarAsync<T1>(sql, ct);
    }

    public async Task<IDbConnection> GetDbConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await DbFactory.OpenDbConnectionAsync(cancellationToken);
    }
}
