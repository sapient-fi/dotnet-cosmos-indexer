using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record TxLogEventAttribute
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
        
    [JsonPropertyName("value")]
    public string Value { get; set; }
}