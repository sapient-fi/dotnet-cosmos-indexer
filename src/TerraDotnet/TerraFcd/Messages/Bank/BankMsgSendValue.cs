using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Bank;

public record BankMsgSendValue
{
    [JsonPropertyName("amount")]
    public List<TerraStringAmount> Amounts { get; set; } = new();
        
    [JsonPropertyName("to_address")]
    public string ToAddress { get; set; } = string.Empty;
        
    [JsonPropertyName("from_address")]
    public string FromAddress { get; set; } = string.Empty;

}