using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record TxFee
{
    [JsonPropertyName("gas")]
    public string Gas { get; set; }
    [JsonPropertyName("amount")]
    public List<TerraStringAmount> Amount { get; set; }
}