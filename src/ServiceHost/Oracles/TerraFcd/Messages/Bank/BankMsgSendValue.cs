using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Bank;

public record BankMsgSendValue
{
    [JsonPropertyName("amount")]
    public List<TerraStringAmount> Amounts { get; set; }
        
    [JsonPropertyName("to_address")]
    public string ToAddress { get; set; }
        
    [JsonPropertyName("from_address")]
    public string FromAddress { get; set; }
}