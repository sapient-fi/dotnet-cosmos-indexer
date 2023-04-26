using Microsoft.Extensions.Logging;
using TerraDotnet;
using TerraDotnet.TerraLcd;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

public class Terra2TransactionEnumerator : CosmosTransactionEnumerator<Terra2Marker>
{
    public Terra2TransactionEnumerator(
        ILogger<CosmosTransactionEnumerator<Terra2Marker>> logger,
        ICosmosLcdApiClient<Terra2Marker> cosmosClient
    ) : base(logger,
        cosmosClient,
        new()
        {
            SecondsPerBlock = 6,
            WindowBlockWidth = 1,
            PaginationLimit = 200
        }
    )
    {
    }
}
