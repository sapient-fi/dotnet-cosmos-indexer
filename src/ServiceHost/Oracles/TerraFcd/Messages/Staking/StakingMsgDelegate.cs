namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Staking;

public record StakingMsgDelegate : IMsg
{
    public string Type { get; set; }
}