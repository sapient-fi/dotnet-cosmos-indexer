using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgSend
{
    [JsonPropertyName("msg")] public string Message { get; set; }

    [JsonPropertyName("amount")] public string Amount { get; set; }

    [JsonPropertyName("contract")] public string Contract { get; set; }
}