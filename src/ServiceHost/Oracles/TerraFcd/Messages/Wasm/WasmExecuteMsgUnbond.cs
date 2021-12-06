using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgUnbond
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; }
    }
}