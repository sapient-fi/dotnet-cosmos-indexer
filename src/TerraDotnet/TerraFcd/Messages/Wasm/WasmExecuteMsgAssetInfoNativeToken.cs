using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMessageAssetInfoNativeToken
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; } = string.Empty;
}