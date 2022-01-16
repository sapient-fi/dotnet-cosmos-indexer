using Pylonboard.ServiceHost.DAL.TerraMoney;

namespace Pylonboard.ServiceHost.Endpoints.MyGatewayPools;

public record MyGatewayPoolResult
{
    public decimal Amount { get; set; }
    public string Denominator { get; set; }
    public TerraPylonPoolOperation Operation { get; set; }
    public TerraPylonPoolFriendlyName FriendlyName { get; set; }
    public string PoolContract { get; set; }
}