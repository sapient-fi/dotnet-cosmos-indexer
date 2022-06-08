namespace Sapient.Kernel.Contracts.Terra;

public record TerraLiquidityPoolPairTransactionMessage
{
    public long TransactionId { get; set; }
    public DecentralizedExchange Dex { get; set; }
}