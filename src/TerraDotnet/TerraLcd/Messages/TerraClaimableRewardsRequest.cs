using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraClaimableRewardsRequest
{
    [JsonPropertyName("claimable_reward")]
    public TerraClaimableRewardBody ClaimableReward { get; set; }
}