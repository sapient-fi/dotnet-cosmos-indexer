using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;

public record TerraBlockResponse
{
    [JsonPropertyName("block_id")]
    public TerraBlockId BlockId { get; set; }

    [JsonPropertyName("block")]
    public TerraBlock Block { get; set; }
}