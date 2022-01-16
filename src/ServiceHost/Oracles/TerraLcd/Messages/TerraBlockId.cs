using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;

public record TerraBlockId
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("parts")]
    public TerraBlockIdParts Parts { get; set; }
}