using System.Text.Json.Serialization;
using TerraDotnet.TerraFcd.Messages;

namespace TerraDotnet.TerraLcd.Messages;

public record CosmosRedelegateMessage : IMsg
{
    [JsonPropertyName("@type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("delegator_address")]
    public string DelegatorAddress { get; set; } = string.Empty;

    [JsonPropertyName("validator_src_address")]
    public string ValidatorSourceAddress { get; set; } = string.Empty;

    [JsonPropertyName("validator_dst_address")]
    public string ValidatorDestinationAddress { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public CosmosAmount? Amount { get; set; }
}