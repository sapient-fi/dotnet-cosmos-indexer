namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;

public record GatewayPoolMineStakerStatsGraph
{
    public IEnumerable<GatewayPoolMineStakerStatsOverallGraph> Overall { get; set; }
}

public record GatewayPoolMineStakerStatsOverallGraph
{
    public decimal? StakingAmount { get; set; }
    public decimal DepositAmount { get; set; }
    public string Depositor { get; set; }
}