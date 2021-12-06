using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record Coin
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}