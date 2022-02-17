using ServiceStack.DataAnnotations;

namespace Pylonboard.Kernel.DAL.Entities.Terra.Views;

[Alias("mv_wallet_stake_percentiles")]
public record MineWalletStakePercentilesView
{
    public decimal P99 { get; set; }
    public decimal P95 { get; set; }
    public decimal P90 { get; set; }
    public decimal P75 { get; set; }
    public decimal Median { get; set; }
    public decimal Average { get; set; }
    public decimal Max { get; set; }
    public decimal Min { get; set; }
};