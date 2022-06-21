using System.Text.Json;
using System.Text.Json.Serialization;
using TerraDotnet.Extensions;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContractValueCol4 : IWasmMsgExecuteContractValue
{
    [JsonPropertyName("coins")]
    public List<Coin> Coins { get; set; } = new();

    [JsonPropertyName("sender")] public string Sender { get; set; } = string.Empty;

    [JsonPropertyName("contract")] public string Contract { get; set; } = string.Empty;

    WasmExecuteMessage? IWasmMsgExecuteContractValue.ExecuteMessage
    {
        get => ExecuteMessageRaw.ToObject<WasmExecuteMessage>();
        set => ExecuteMessageRaw = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(value));
    }

    [JsonPropertyName("execute_msg")] public string ExecuteMessageRaw { get; set; } = string.Empty;
}