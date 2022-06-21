using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperationsOperationTerraSwap
{
    [JsonPropertyName("ask_asset_info")]
    public WasmExecuteMessageAssetInfo AskAssetInfo { get; set; } = new();

    [JsonPropertyName("offer_asset_info")]
    public WasmExecuteMessageAssetInfo OfferAssetInfo { get; set; } = new();
}