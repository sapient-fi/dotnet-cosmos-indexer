using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record LcdTxResponse
{
    public string Height { get; set; } = string.Empty;

    public int HeightAsInt => int.Parse(Height);

    [JsonPropertyName("txhash")]
    public string TransactionHash { get; set; } = string.Empty;

    [JsonPropertyName("codespace")]
    public string CodeSpace { get; set; } = string.Empty;

    public int Code { get; set; }
    
    public string Data { get; set; } = string.Empty;
    
    [JsonPropertyName("raw_log")]
    public string RawLog { get; set; } = string.Empty;
    
    public List<LcdTxResponseLog> Logs { get; set; }

    public string Info { get; set; } = string.Empty;

    [JsonPropertyName("gas_wanted")]
    public string RawGasWanted { get; set; } = string.Empty;

    public int GasWanted => int.Parse(RawGasWanted);

    [JsonPropertyName("gas_used")]
    public string RawGasUsed { get; set; } = string.Empty;

    public int GasUsed => int.Parse(RawGasUsed);

    [JsonPropertyName("tx")]
    public LcdTx Transaction { get; set; } = new();
    
    [JsonPropertyName("timestamp")]
    public DateTimeOffset CreatedAt { get; set; }

    public List<LcdEvent> Events { get; set; } = new();
}