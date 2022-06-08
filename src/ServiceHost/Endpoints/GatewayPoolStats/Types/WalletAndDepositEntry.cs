namespace Sapient.ServiceHost.Endpoints.GatewayPoolStats.Types;

public record WalletAndDepositEntry
{
    public string Wallet { get; set; }

    public decimal Amount { get; set; }

    public decimal InPercent { get; set; }
}