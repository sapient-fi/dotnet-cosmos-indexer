using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgAirdrop
{
    [JsonPropertyName("claim")]
    public WasmExecuteMsgAirdropClaim Claim { get; set; }
}