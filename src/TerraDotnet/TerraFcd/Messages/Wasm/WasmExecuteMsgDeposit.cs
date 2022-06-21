using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgDeposit
{
    public WasmExecuteMessageAsset Asset { get; set; } = new();
        
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}