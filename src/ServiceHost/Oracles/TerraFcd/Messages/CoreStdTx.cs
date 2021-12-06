using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record CoreStdTx
{
    public long Id { get; set; }
    public string TransactionHash { get; set; }
        
    [JsonPropertyName("fee")]
    public TxFee Fee { get; set; }
        
    [JsonPropertyName("msg")]
    public List<IMsg> Messages { get; set; }

    [JsonPropertyName("memo")]
    public string Memo { get; set; }

    public List<TxLog> Logs { get; set; }
        
    // TODO Signatures 
    // TODO timeout_height
}