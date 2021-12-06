using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMessageAsset
    {
        [JsonPropertyName("info")]
        public WasmExecuteMessageAssetInfo Info { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }
    }
}