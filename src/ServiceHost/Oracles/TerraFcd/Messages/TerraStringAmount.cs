using System.Text.Json.Serialization;
using Pylonboard.ServiceHost.Extensions;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record TerraStringAmount
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; }
        
    [JsonPropertyName("amount")]
    public string Amount { get; set; }

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