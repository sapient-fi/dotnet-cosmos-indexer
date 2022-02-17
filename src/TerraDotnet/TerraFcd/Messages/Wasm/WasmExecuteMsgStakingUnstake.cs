using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgStakingUnstake
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}