using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;

public record TerraClaimableRewardBody
{
    [JsonPropertyName("owner")]
    public string Owner { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}