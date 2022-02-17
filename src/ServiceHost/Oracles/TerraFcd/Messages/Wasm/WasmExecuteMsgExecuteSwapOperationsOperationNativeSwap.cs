using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperationsOperationNativeSwap
{
    [JsonPropertyName("offer_denom")]
    public string OfferDenom { get; set; }
    [JsonPropertyName("ask_denom")]
    public string AskDenom { get; set; }
}