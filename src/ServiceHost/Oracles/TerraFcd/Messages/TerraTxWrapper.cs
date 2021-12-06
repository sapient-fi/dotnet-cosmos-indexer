using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

public record TerraTxWrapper
{
    /// <summary>
    /// Unique id of the transaction
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// Id of the chain it was performed on 
    /// </summary>
    public string ChainId { get; set; }

    /// <summary>
    /// A generic transaction carrier 
    /// </summary>
    public TerraTxGeneric Tx { get; set; }

    /// <summary>
    /// Block height at which this transaction happened
    /// </summary>
    [JsonPropertyName("height")]
    public string Height { get; set; }
        
    /// <summary>
    /// Transaction hash
    /// </summary>
    [JsonPropertyName("txhash")]
    public string TransactionHash { get; set; }
        
    [JsonPropertyName("gas_used")]
    public string GasUsed { get; set; }
        
    [JsonPropertyName("gas_wanted")]
    public string GasWanted { get; set; }
        
    [JsonPropertyName("timestamp")]
    public DateTimeOffset CreatedAt { get; set; }
        
    /// <summary>
    /// Code 0 means all good, other codes means some kind of failure
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("raw_log")]
    public string RawLog { get; set; }

    [JsonPropertyName("logs")]
    public List<TxLog> Logs { get; set; }
}