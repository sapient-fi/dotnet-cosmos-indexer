using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgStakingUnstake
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}