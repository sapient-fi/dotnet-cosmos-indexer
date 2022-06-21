using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperationsOperationNativeSwap
{
    [JsonPropertyName("offer_denom")]
    public string OfferDenom { get; set; } = string.Empty;
    [JsonPropertyName("ask_denom")]
    public string AskDenom { get; set; } = string.Empty;
}