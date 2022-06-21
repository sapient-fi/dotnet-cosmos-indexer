namespace TerraDotnet.TerraFcd.Messages.Market;

public record MarketMsgSwap : IMsg
{
    public string Type { get; set; } = string.Empty;
}