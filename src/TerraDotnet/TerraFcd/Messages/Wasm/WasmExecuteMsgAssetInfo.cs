using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMessageAssetInfo
{
    [JsonPropertyName("token")]
    public WasmExecuteMessageAssetInfoToken Token { get; set; } = new();

    [JsonPropertyName("native_token")]
    public WasmExecuteMessageAssetInfoNativeToken NativeToken { get; set; } = new();
}