namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Market;

public record MarketMsgSwap : IMsg
{
    public string Type { get; set; }
}