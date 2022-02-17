namespace TerraDotnet.TerraFcd.Messages.Staking;

public record StakingMsgDelegate : IMsg
{
    public string Type { get; set; }
}