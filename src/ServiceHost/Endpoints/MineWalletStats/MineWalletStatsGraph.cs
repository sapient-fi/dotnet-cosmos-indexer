namespace Sapient.ServiceHost.Endpoints.MineWalletStats;

public class MineWalletStatsGraph
{
    public string Wallet { get; set; }

    public decimal Sum { get; set; }

    public DateTimeOffset StakedSince { get; set; }
}