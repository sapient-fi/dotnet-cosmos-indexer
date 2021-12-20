using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.TerraMoney.Views;

[Alias("v_wallet_stake_sum_v2")]
public record MineWalletStakeViewV2
{
    public string Wallet { get; set; }
    
    public decimal Sum { get; set; }

    public DateTimeOffset StakedSince { get; set; }
};