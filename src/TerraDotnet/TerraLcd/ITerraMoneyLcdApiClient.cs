using Refit;
using TerraDotnet.TerraLcd.Messages;

namespace TerraDotnet.TerraLcd;

public interface ITerraMoneyLcdApiClient
{
    [Get("/blocks/latest")]
    public Task<ApiResponse<TerraBlockResponse>> GetLatestBlockAsync();

    [Get("/cosmos/tx/v1beta1/txs")]
    public Task<GetTransactionsMatchingQueryResponse> GetTransactionsMatchingQueryAsync(
        [AliasAs("events"), Query(CollectionFormat.Multi)] string[] conditions,
        [AliasAs("pagination.limit"), Query("pagination.limit")] int paginationLimit = 100,
        [AliasAs("pagination.offset"), Query("pagination.offset")] int paginationOffset = 0
    );
}