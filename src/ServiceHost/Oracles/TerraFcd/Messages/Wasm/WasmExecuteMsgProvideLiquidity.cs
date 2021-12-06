using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgProvideLiquidity
    {
        [JsonPropertyName("assets")] public List<WasmExecuteMessageAsset> Assets { get; set; }
        
        [JsonPropertyName("slippage_tolerance")]
        public string SlippageTolerance { get; set; }
        
        [JsonPropertyName("token_amount")] 
        public string TokenAmount { get; set; }
    }
}