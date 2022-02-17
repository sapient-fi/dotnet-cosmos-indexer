using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraPylonPoolQueryResponse
{
    [JsonPropertyName("height")]
    public string Height { get; set; }

    [JsonPropertyName("result")]
    public TerraPylonPoolQueryResult Result { get; set; }
}