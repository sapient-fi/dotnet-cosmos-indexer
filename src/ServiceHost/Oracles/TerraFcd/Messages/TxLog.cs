using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record TxLog
{
    [JsonPropertyName("events")]
    public List<TxLogEvent> Events { get; set; }
}