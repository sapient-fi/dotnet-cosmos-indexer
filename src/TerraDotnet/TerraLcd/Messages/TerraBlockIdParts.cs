using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraBlockIdParts
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}