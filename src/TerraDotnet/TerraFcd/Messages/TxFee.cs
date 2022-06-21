using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TxFee
{
    [JsonPropertyName("gas")]
    public string Gas { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public List<TerraStringAmount> Amount { get; set; } = new();
}