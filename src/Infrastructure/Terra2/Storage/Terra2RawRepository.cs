using SapientFi.Infrastructure.Cosmos.Storage;
using ServiceStack.Data;

namespace SapientFi.Infrastructure.Terra2.Storage;

public class Terra2RawRepository : CosmosRawRepository<Terra2RawTransactionEntity>
{
    public Terra2RawRepository(IDbConnectionFactory dbFactory) : base(dbFactory)
    {
    }
}