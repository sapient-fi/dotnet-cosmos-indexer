using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.TerraMoney.Views;

[Alias("mv_wallet_stake_sum")]
public record MineWalletStakeView
{
    public string Wallet { get; set; }
    
    public decimal Sum { get; set; }

    public DateTimeOffset StakedSince { get; set; }
};