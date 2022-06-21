using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgAirdrop
{
    [JsonPropertyName("claim")]
    public WasmExecuteMsgAirdropClaim Claim { get; set; } = new();
}