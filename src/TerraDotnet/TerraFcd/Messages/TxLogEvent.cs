using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TxLogEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("attributes")]
    public List<TxLogEventAttribute> Attributes { get; set; } = new();
}