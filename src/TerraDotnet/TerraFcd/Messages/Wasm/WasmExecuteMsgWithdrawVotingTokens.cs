using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgWithdrawVotingTokens
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}