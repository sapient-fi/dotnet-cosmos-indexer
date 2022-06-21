using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record CoreStdTx
{
    public long Id { get; set; }
    public string TransactionHash { get; set; } = string.Empty;

    [JsonPropertyName("fee")]
    public TxFee Fee { get; set; } = new();

    [JsonPropertyName("msg")]
    public List<IMsg> Messages { get; set; } = new();

    [JsonPropertyName("memo")]
    public string Memo { get; set; } = string.Empty;

    public List<TxLog> Logs { get; set; } = new();

    // TODO Signatures 
    // TODO timeout_height
}