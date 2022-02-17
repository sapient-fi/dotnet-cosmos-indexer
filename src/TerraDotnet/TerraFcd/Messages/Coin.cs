using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record Coin
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}