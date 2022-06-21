using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraBlockHeader
{
    [JsonPropertyName("chain_id")]
    public string ChainId { get; set; } = string.Empty;
    
    [JsonPropertyName("height")]
    public string Height { get; set; } = string.Empty;

    public int HeightAsInt => int.Parse(Height);

    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }

    [JsonPropertyName("last_block_id")]
    public TerraBlockId LastBlockId { get; set; } = new();
}