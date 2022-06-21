using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TxLogEventAttribute
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
        
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}