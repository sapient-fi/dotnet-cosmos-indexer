using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMessageAssetInfoToken
{
    [JsonPropertyName("contract_addr")]
    public string ContractAddress { get; set; }
}