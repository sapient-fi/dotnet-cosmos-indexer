using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record CosmosBlockId
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("parts")]
    public CosmosBlockIdParts Parts { get; set; } = new();
}