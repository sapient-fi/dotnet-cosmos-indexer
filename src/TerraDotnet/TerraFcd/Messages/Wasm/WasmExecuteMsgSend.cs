using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgSend
{
    [JsonPropertyName("msg")] public string? Message { get; set; }

    [JsonPropertyName("amount")] public string? Amount { get; set; }

    [JsonPropertyName("contract")] public string? Contract { get; set; }
}