using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record Coin
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}