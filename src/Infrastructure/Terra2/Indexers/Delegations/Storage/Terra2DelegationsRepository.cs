using SapientFi.Infrastructure.Cosmos.Indexers.Delegations.Storage;
using ServiceStack.Data;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;

public class Terra2DelegationsRepository : CosmosDelegationsRepository<Terra2ValidatorDelegationLedgerEntity>
{
    public Terra2DelegationsRepository(IDbConnectionFactory dbFactory) : base(dbFactory)
    {
    }
}