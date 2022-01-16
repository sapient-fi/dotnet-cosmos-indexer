using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraLcd.Messages;

public record TerraClaimableRewardsRequest
{
    [JsonPropertyName("claimable_reward")]
    public TerraClaimableRewardBody ClaimableReward { get; set; }
}