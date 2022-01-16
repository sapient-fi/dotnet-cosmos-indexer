using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;

public record TerraBlock
{
    [JsonPropertyName("header")]
    public TerraBlockHeader Header { get; set; }
    
}