using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgStaking
    {
        [JsonPropertyName("unstake")] public WasmExecuteMsgStakingUnstake Unstake { get; set; }
    }
}