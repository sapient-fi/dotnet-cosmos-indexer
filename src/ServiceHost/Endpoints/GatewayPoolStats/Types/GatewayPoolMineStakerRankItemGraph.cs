namespace Sapient.ServiceHost.Endpoints.GatewayPoolStats.Types;

public record GatewayPoolMineStakerRankItemGraph
{
    public decimal DepositAmountMedian { get; set; }
    public decimal DepositAmountAvg { get; set; }
    public decimal DepositAmountSum { get; set; }
    public decimal DepositAmountMin { get; set; }
    public decimal DepositAmountMax { get; set; }
    public decimal StakingLowerBound { get; set; }
    public decimal StakingUpperBound { get; set; }
}