using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TxFee
{
    [JsonPropertyName("gas")]
    public string Gas { get; set; }
    [JsonPropertyName("amount")]
    public List<TerraStringAmount> Amount { get; set; }
}