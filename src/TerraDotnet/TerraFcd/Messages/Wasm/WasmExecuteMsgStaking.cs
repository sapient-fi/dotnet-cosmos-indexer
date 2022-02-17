using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgStaking
{
    [JsonPropertyName("unstake")] public WasmExecuteMsgStakingUnstake Unstake { get; set; }
}