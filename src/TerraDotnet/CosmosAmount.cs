using System.Text.Json.Serialization;

namespace TerraDotnet;

public record CosmosAmount
{
    [JsonPropertyName("denom")]
    public string Denominator { get; set; } = string.Empty;
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}
