using System.Threading.Tasks;
using Refit;
using TerraDotnet.TerraLcd.Messages;

namespace TerraDotnet.TerraLcd;

public interface ICosmosLcdApiClient<T> 
{
    [Get("/cosmos/base/tendermint/v1beta1/blocks/latest")]
    public Task<ApiResponse<CosmosBlockResponse>> GetLatestBlockAsync();

    [Get("/cosmos/tx/v1beta1/txs")]
    public Task<GetTransactionsMatchingQueryResponse> GetTransactionsMatchingQueryAsync(
        [AliasAs("events"), Query(CollectionFormat.Multi)] string[] conditions,
        [AliasAs("pagination.limit"), Query("pagination.limit")] int paginationLimit = 100,
        [AliasAs("pagination.offset"), Query("pagination.offset")] int paginationOffset = 0
    );
}