using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgDeposit
{
    public WasmExecuteMessageAsset Asset { get; set; }
        
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}