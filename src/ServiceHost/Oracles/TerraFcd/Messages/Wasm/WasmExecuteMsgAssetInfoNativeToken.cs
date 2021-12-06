using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMessageAssetInfoNativeToken
    {
        [JsonPropertyName("denom")]
        public string Denominator { get; set; }
    }
}