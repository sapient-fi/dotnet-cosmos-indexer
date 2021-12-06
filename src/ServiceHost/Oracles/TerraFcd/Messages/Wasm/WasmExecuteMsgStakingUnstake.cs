using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgStakingUnstake
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; }
    }
}