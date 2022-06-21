using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraClaimableRewardBody
{
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}