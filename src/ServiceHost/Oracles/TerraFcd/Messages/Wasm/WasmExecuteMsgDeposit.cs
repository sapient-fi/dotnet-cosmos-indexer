using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgDeposit
{
    public WasmExecuteMessageAsset Asset { get; set; }
        
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}