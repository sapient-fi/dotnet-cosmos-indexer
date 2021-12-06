using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMessageAssetInfo
    {
        [JsonPropertyName("token")]
        public WasmExecuteMessageAssetInfoToken Token { get; set; }
        [JsonPropertyName("native_token")]
        public WasmExecuteMessageAssetInfoNativeToken NativeToken { get; set; } 
    }
}