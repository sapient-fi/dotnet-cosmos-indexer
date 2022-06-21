using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContractValueCol5 : IWasmMsgExecuteContractValue
{
    [JsonPropertyName("coins")]
    public List<Coin> Coins { get; set; } = new();

    [JsonPropertyName("sender")] public string Sender { get; set; } = string.Empty;

    [JsonPropertyName("contract")] public string Contract { get; set; } = string.Empty;

    [JsonPropertyName("execute_msg")] public WasmExecuteMessage? ExecuteMessage { get; set; }
}