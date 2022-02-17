using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraBlock
{
    [JsonPropertyName("header")]
    public TerraBlockHeader Header { get; set; }
    
}