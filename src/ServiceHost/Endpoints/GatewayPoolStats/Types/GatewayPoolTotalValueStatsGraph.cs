namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;

public record GatewayPoolTotalValueStatsGraph
{
    public decimal TotalValueLocked { get; set; }
    public decimal ValueLocked24h { get; set; }
    public decimal ValueLocked7d { get; set; }
    public decimal ValueLocked30d { get; set; }
}