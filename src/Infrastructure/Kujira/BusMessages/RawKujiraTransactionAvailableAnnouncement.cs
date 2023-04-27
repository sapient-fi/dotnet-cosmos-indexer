using System;
using SapientFi.Infrastructure.Cosmos.BusMessages;

namespace SapientFi.Infrastructure.Kujira.BusMessages;

public record RawKujiraTransactionAvailableAnnouncement : IRawCosmosTransactionAvailableAnnouncement
{
    public string TransactionHash { get; init; } = string.Empty;
    public long RawEntityId { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
}
