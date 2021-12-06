using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContractValueCol5 : IWasmMsgExecuteContractValue
{
    [JsonPropertyName("coins")] public List<Coin> Coins { get; set; }

    [JsonPropertyName("sender")] public string Sender { get; set; }

    [JsonPropertyName("contract")] public string Contract { get; set; }

    [JsonPropertyName("execute_msg")] public WasmExecuteMessage ExecuteMessage { get; set; }
}