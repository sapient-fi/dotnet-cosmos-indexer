using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMessageAsset
{
    [JsonPropertyName("info")]
    public WasmExecuteMessageAssetInfo Info { get; set; } = new();

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}