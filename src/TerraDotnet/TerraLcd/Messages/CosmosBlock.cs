using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record CosmosBlock
{
    [JsonPropertyName("header")]
    public CosmosBlockHeader Header { get; set; } = new();

}