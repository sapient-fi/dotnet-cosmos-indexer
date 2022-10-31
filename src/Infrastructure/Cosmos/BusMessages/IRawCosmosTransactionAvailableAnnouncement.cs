namespace SapientFi.Infrastructure.Cosmos.BusMessages;

public interface IRawCosmosTransactionAvailableAnnouncement
{
    public string TransactionHash { get; init; }
    public long RawEntityId { get; init; }
}
