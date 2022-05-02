using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgWithdraw
{
    [JsonPropertyName("asset")]
    public WasmExecuteMessageAsset? Asset { get; init; }
        
    [JsonPropertyName("amount")] 
    public string? Amount { get; init; }
}