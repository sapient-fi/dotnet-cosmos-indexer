namespace Sapient.ServiceHost.Endpoints.GatewayPoolStats.Types;

public record GatewayPoolStatsGraph
{
    public GatewayPoolStatsOverallGraph Overall { get; set; }
}