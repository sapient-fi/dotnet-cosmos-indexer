using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMessageAssetInfo
{
    [JsonPropertyName("token")]
    public WasmExecuteMessageAssetInfoToken Token { get; set; }
    [JsonPropertyName("native_token")]
    public WasmExecuteMessageAssetInfoNativeToken NativeToken { get; set; } 
}