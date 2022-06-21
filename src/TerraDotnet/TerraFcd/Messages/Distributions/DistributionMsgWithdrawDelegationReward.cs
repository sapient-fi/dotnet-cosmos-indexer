namespace TerraDotnet.TerraFcd.Messages.Distributions;

public record DistributionMsgWithdrawDelegationReward : IMsg
{
    public string Type { get; set; } = string.Empty;
    public DistributionWithdrawDelegationRewardValue Value { get; set; } = new();
}