using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperationsOperationTerraSwap
{
    [JsonPropertyName("ask_asset_info")]
    public WasmExecuteMessageAssetInfo AskAssetInfo { get; set; }
    
    [JsonPropertyName("offer_asset_info")]
    public WasmExecuteMessageAssetInfo OfferAssetInfo { get; set; }
}