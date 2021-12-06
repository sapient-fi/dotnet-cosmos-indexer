using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgDeposit
    {
        public WasmExecuteMessageAsset Asset { get; set; }
        
        [JsonPropertyName("amount")]
        public string Amount { get; set; }
    }
}