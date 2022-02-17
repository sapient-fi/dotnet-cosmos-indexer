using ServiceStack.DataAnnotations;

namespace Pylonboard.Kernel.DAL.Entities.Terra.Views;

[Alias("mv_my_gateway_pools")]
public record MyGatewayPoolsView
{
    public decimal Amount { get; set; }
    public string Denominator { get; set; }
    public TerraPylonPoolOperation Operation { get; set; }
    public TerraPylonPoolFriendlyName FriendlyName { get; set; }
    public string PoolContract { get; set; }
    public string Depositor { get; set; }
}