using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMessageAssetInfoNativeToken
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; }
}