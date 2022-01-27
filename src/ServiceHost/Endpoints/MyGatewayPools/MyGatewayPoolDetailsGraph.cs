using Pylonboard.ServiceHost.DAL.TerraMoney;

namespace Pylonboard.ServiceHost.Endpoints.MyGatewayPools;

public record MyGatewayPoolDetailsGraph
{
    public decimal Amount { get; set; }
    public string Denominator { get; set; }
    public TerraPylonPoolOperation Operation { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string TxHash { get; set; }
}