namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;

public record GatewayPoolStatsGraph
{
    public GatewayPoolStatsOverallGraph Overall { get; set; }
}