using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record CosmosClaimableRewardsRequest
{
    [JsonPropertyName("claimable_reward")]
    public CosmosClaimableRewardBody ClaimableReward { get; set; } = new();
}