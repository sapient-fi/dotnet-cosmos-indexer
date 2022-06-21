using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgSwap
{
    [JsonPropertyName("max_spread")]
    public string MaxSpread { get; set; } = string.Empty;

    [JsonPropertyName("offer_spread")]
    public WasmExecuteMessageAsset OfferAsset { get; set; } = new();
        
    [JsonPropertyName("belief_price")]
    public string BeliefPrice { get; set; } = string.Empty;
}