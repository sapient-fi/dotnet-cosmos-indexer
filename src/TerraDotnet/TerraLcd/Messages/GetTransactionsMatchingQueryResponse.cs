using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record GetTransactionsMatchingQueryResponse
{
    /// <summary>
    /// These are fairly "light-weight", so what you probably want
    /// is the <see cref="TransactionResponses"/>
    /// </summary>
    [JsonPropertyName("txs")]
    public List<LcdTx> Transactions { get; set; } = new();

    [JsonPropertyName("tx_responses")]
    public List<LcdTxResponse> TransactionResponses { get; set; } = new();

    public LcdPagination Pagination { get; set; } = new();
}