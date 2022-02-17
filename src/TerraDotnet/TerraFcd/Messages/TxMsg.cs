using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TxMsg
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}