using System.Text.Json.Serialization;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace TerraDotnet.TerraFcd.Messages;

public record TerraStringAmount
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; } = string.Empty;
        
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    public WasmExecuteMessageAsset ToWasmExecuteMessageAsset()
    {
        if (Denominator.IsMuDemominator())
        {
            return new WasmExecuteMessageAsset
            {
                Amount = Amount,
                Info = new WasmExecuteMessageAssetInfo
                {
                    NativeToken = new WasmExecuteMessageAssetInfoNativeToken
                    {
                        Denominator = Denominator
                    }
                }
            };
        }

        return new WasmExecuteMessageAsset
        {
            Amount = Amount,
            Info = new WasmExecuteMessageAssetInfo
            {
                Token = new WasmExecuteMessageAssetInfoToken
                {
                    ContractAddress = Denominator,
                }
            }
        };
    }
}