namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;

public record WalletAndDepositEntry
{
    public string Wallet { get; set; }

    public decimal Amount { get; set; }

    public decimal InPercent { get; set; }
}