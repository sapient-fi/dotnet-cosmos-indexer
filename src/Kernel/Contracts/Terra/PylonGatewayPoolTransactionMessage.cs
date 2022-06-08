using Sapient.Kernel.DAL.Entities.Terra;

namespace Sapient.Kernel.Contracts.Terra;

public record PylonGatewayPoolTransactionMessage
{
    public long TransactionId { get; init; }
    
    public string PoolContractAddr { get; init; }
    
    public TerraPylonPoolFriendlyName FriendlyName { get; init; }
}