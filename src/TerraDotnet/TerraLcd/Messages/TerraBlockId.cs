using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraBlockId
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("parts")]
    public TerraBlockIdParts Parts { get; set; }
}