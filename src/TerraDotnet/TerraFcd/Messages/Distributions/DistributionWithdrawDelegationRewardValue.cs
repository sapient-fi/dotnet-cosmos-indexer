using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Distributions;

public record DistributionWithdrawDelegationRewardValue
{
    [JsonPropertyName("delegator_address")]
    public string DelegatorAddress { get; set; } = string.Empty;

    [JsonPropertyName("validator_address")]
    public string ValidatorAddress { get; set; } = string.Empty;
}