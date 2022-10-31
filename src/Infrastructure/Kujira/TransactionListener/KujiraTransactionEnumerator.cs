using Microsoft.Extensions.Logging;
using TerraDotnet;
using TerraDotnet.TerraLcd;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

public class KujiraTransactionEnumerator : CosmosTransactionEnumerator<KujiraMarker>
{
    public KujiraTransactionEnumerator(
        ILogger<CosmosTransactionEnumerator<KujiraMarker>> logger,
        ICosmosLcdApiClient<KujiraMarker> cosmosClient
    ) : base(logger,
        cosmosClient,
        new()
        {
            SecondsPerBlock = 6,
            WindowBlockWidth = 2000,
            PaginationLimit = 200
        }
    )
    {
    }
}
