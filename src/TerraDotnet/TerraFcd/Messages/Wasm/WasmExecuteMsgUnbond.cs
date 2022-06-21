using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgUnbond
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}