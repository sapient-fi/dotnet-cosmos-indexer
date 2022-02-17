using System.Text.Json;
using System.Text.Json.Serialization;
using Pylonboard.ServiceHost.Extensions;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContractValueCol4 : IWasmMsgExecuteContractValue
{
    [JsonPropertyName("coins")] public List<Coin> Coins { get; set; }

    [JsonPropertyName("sender")] public string Sender { get; set; }

    [JsonPropertyName("contract")] public string Contract { get; set; }

    WasmExecuteMessage? IWasmMsgExecuteContractValue.ExecuteMessage
    {
        get => ExecuteMessageRaw.ToObject<WasmExecuteMessage>();
        set => ExecuteMessageRaw = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(value));
    }

    [JsonPropertyName("execute_msg")] public string ExecuteMessageRaw { get; set; }
}