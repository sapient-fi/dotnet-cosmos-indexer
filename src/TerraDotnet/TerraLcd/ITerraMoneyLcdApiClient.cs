using Refit;
using TerraDotnet.TerraLcd.Messages;

namespace TerraDotnet.TerraLcd;

public interface ITerraMoneyLcdApiClient
{

    [Get("/blocks/latest")]
    public Task<ApiResponse<TerraBlockResponse>> GetLatestBlockAsync();

    [Get("/wasm/contracts/{poolContractId}/store?query_msg={queryObj}")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    public Task<TerraPylonPoolQueryResponse> FetchPylonPoolDataAsync(
        [AliasAs("poolContractId")] string poolContractId,
        [AliasAs("queryObj")] string queryObject
    );
}