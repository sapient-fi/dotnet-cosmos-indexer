using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record CosmosBlockResponse
{
    [JsonPropertyName("block_id")]
    public CosmosBlockId BlockId { get; set; } = new();

    [JsonPropertyName("block")]
    public CosmosBlock Block { get; set; } = new();
}