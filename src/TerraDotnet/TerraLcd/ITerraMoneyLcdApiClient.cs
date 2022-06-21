using Refit;
using TerraDotnet.TerraLcd.Messages;

namespace TerraDotnet.TerraLcd;

public interface ITerraMoneyLcdApiClient
{
    [Get("/blocks/latest")]
    public Task<ApiResponse<TerraBlockResponse>> GetLatestBlockAsync();
}