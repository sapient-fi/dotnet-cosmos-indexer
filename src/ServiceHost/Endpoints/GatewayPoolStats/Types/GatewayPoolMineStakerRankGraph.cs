namespace Sapient.ServiceHost.Endpoints.GatewayPoolStats.Types;

public record GatewayPoolMineStakerRankGraph
{ 
    public GatewayPoolMineStakerRankItemGraph Tier1 { get; set; }
    public GatewayPoolMineStakerRankItemGraph Tier2 { get; set; }
    public GatewayPoolMineStakerRankItemGraph Tier3 { get; set; }
    public GatewayPoolMineStakerRankItemGraph Tier4 { get; set; }
    public GatewayPoolMineStakerRankItemGraph Tier5 { get; set; }
}