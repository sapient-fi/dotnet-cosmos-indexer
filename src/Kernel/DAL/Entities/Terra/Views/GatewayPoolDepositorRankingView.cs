using ServiceStack.DataAnnotations;

namespace Pylonboard.Kernel.DAL.Entities.Terra.Views;

[Alias("mv_gateway_pool_staker_ranking")]
public record GatewayPoolDepositorRankingView
{
    public decimal DepositAmountMedian { get; set; }
    public decimal DepositAmountAvg { get; set; }
    public decimal DepositAmountSum { get; set; }
    public decimal DepositAmountMin { get; set; }
    public decimal DepositAmountMax { get; set; }
    public decimal StakingAmount { get; set; }
    public TerraPylonPoolFriendlyName FriendlyName { get; set; }
};