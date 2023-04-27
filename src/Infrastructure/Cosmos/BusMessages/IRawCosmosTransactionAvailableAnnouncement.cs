using System;

namespace SapientFi.Infrastructure.Cosmos.BusMessages;

public interface IRawCosmosTransactionAvailableAnnouncement
{
    public string TransactionHash { get; init; }
    /// <summary>
    /// Primary key id in the underlying database
    /// </summary>
    public long RawEntityId { get; init; }
    
    /// <summary>
    /// Timestamp in the entity for it's creation date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}