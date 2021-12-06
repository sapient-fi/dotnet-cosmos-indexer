using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgStaking
{
    [JsonPropertyName("unstake")] public WasmExecuteMsgStakingUnstake Unstake { get; set; }
}