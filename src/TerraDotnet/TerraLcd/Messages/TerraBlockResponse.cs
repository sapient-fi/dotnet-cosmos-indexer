using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraBlockResponse
{
    [JsonPropertyName("block_id")]
    public TerraBlockId BlockId { get; set; }

    [JsonPropertyName("block")]
    public TerraBlock Block { get; set; }
}