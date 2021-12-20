namespace Pylonboard.ServiceHost.Endpoints;

public class MineWalletStatsGraph
{
    public string Wallet { get; set; }

    public decimal Sum { get; set; }

    public DateTimeOffset StakedSince { get; set; }
}