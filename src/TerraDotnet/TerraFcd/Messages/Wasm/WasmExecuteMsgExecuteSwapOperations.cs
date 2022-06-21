using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperations
{
    [JsonPropertyName("operations")]
    public List<WasmExecuteMsgExecuteSwapOperationsOperation> Operations { get; set; } = new();

    [JsonPropertyName("offer_amount")]
    public string? OfferAmount { get; set; }

    [JsonPropertyName("MinimumReceive")]
    public string? MinimumReceive { get; set; }
    
}