using SapientFi.Infrastructure.Cosmos.BusMessages;

namespace SapientFi.Infrastructure.Terra2.BusMessages;

public class RawTerra2TransactionAvailableAnnouncement : IRawCosmosTransactionAvailableAnnouncement
{
    public string TransactionHash { get; init; } = string.Empty;
    public long RawEntityId { get; init; }
}