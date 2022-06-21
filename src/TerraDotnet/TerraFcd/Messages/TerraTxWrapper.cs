using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages;

public record TerraTxWrapper
{
    /// <summary>
    /// Unique id of the transaction
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// Id of the chain it was performed on 
    /// </summary>
    public string ChainId { get; set; } = string.Empty;

    /// <summary>
    /// A generic transaction carrier 
    /// </summary>
    public TerraTxGeneric Tx { get; set; } = new();

    /// <summary>
    /// Block height at which this transaction happened
    /// </summary>
    [JsonPropertyName("height")]
    public string Height { get; set; } = string.Empty;
        
    /// <summary>
    /// Transaction hash
    /// </summary>
    [JsonPropertyName("txhash")]
    public string TransactionHash { get; set; } = string.Empty;
        
    [JsonPropertyName("gas_used")]
    public string GasUsed { get; set; } = string.Empty;
        
    [JsonPropertyName("gas_wanted")]
    public string GasWanted { get; set; } = string.Empty;
        
    [JsonPropertyName("timestamp")]
    public DateTimeOffset CreatedAt { get; set; }
        
    /// <summary>
    /// Code 0 means all good, other codes means some kind of failure
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("logs")]
    public List<TxLog> Logs { get; set; } = new();
}