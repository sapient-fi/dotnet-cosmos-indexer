using SapientFi.Infrastructure.Cosmos.Storage;
using ServiceStack.Data;

namespace SapientFi.Infrastructure.Kujira.Storage;

public class KujiraRawRepository : CosmosRawRepository<KujiraRawTransactionEntity>
{
    public KujiraRawRepository(IDbConnectionFactory dbFactory) : base(dbFactory)
    {
    }
}
