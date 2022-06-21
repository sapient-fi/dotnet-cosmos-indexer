using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TxLog
{
    [JsonPropertyName("events")]
    public List<TxLogEvent> Events { get; set; } = new();
}