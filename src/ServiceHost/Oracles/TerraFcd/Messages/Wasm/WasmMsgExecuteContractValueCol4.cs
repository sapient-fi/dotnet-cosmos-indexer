using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContractValueCol4 : IWasmMsgExecuteContractValue
{
    [JsonPropertyName("coins")] public List<Coin> Coins { get; set; }

    [JsonPropertyName("sender")] public string Sender { get; set; }

    [JsonPropertyName("contract")] public string Contract { get; set; }

    WasmExecuteMessage IWasmMsgExecuteContractValue.ExecuteMessage
    {
        get
        {
            var rawJson = Convert.FromBase64String(ExecuteMessageRaw);
            return JsonSerializer.Deserialize<WasmExecuteMessage>(rawJson.AsSpan());
        }
        set => ExecuteMessageRaw = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(value));
    }

    [JsonPropertyName("execute_msg")] public string ExecuteMessageRaw { get; set; }
}