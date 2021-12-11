using Pylonboard.ServiceHost.Endpoints.Types;

namespace Pylonboard.ServiceHost.Endpoints.GatewayPoolStats;

public record GatewayPoolStatsOverallGraph
{
    public decimal TotalValueLocked { get; set; }

    public decimal AverageDeposit { get; set; }

    public decimal MaxDeposit { get; set; }
    
    public decimal MinDeposit { get; set; }

    public List<WalletAndDepositEntry> DepositPerWallet { get; set; }

    public List<TimeSeriesStatEntry> DepositsOverTime { get; set; }
    
    
}