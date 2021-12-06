using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Distributions;

public record DistributionWithdrawDelegationRewardValue
{
    [JsonPropertyName("delegator_address")]
    public string DelegatorAddress { get; set; }

    [JsonPropertyName("validator_address")]
    public string ValidatorAddress { get; set; }
}