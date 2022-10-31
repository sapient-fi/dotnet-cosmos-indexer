using SapientFi.Infrastructure.Cosmos.Indexers.Delegations.Storage;
using ServiceStack.Data;

namespace SapientFi.Infrastructure.Kujira.Indexers.Delegations.Storage;

public class KujiraDelegationsRepository : CosmosDelegationsRepository<KujiraValidatorDelegationLedgerEntity>
{
    public KujiraDelegationsRepository(IDbConnectionFactory dbFactory) : base(dbFactory)
    {
    }
}
