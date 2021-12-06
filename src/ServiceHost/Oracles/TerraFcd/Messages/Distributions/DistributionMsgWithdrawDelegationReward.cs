namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Distributions
{
    public record DistributionMsgWithdrawDelegationReward : IMsg
    {
        public string Type { get; set; }
        public DistributionWithdrawDelegationRewardValue Value { get; set; }
    }
}