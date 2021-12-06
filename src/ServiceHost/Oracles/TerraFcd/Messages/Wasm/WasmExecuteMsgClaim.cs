using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgClaim
    {
        [JsonPropertyName("stage")]
        public int Stage { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }
    }
}