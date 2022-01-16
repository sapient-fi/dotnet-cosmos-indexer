using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;

public record TerraBlockHeader
{
    [JsonPropertyName("chain_id")]
    public string ChainId { get; set; }
    
    [JsonPropertyName("height")]
    public string Height { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }

    [JsonPropertyName("last_block_id")]
    public TerraBlockId LastBlockId { get; set; }
}