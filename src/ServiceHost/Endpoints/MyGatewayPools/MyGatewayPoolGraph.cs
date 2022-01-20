using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Endpoints.GatewayPoolStats.Types;

namespace Pylonboard.ServiceHost.Endpoints.MyGatewayPools;

public record MyGatewayPoolGraph
{
    public decimal TotalDepositAmount { get; set; }
    public decimal TotalWithdrawnAmount { get; set; }
    public decimal TotalClaimedAmount { get; set; }
    public decimal TotalClaimedAmountInUst { get; set; }
    public string RewardDenominator { get; set; }
    public string PoolContractAddress { get; set; }
    public GatewayPoolIdentifier PoolIdentifier { get; set; }
    public TerraPylonPoolFriendlyName FriendlyName { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset ClaimAt { get; set; }
    public DateTimeOffset WithdrawAt { get; set; }
}