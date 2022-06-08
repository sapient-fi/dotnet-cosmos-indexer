using ServiceStack.DataAnnotations;

namespace Sapient.Kernel.DAL.Entities.Terra.Views;

[Alias("mv_wallet_stake_sum")]
public record MineWalletStakeView
{
    public string Wallet { get; set; }
    
    public decimal Sum { get; set; }

    public DateTimeOffset StakedSince { get; set; }
};